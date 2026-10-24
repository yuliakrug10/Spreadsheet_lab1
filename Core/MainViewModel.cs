using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Spreadsheet_lab1.Core
{
    public class MainViewModel
    {
        private Dictionary<string, Cell> _cells = new Dictionary<string, Cell>();
        private int _rowNumber;
        private int _columnNumber;

        private readonly CyclesTracker _tracker = new CyclesTracker();
        private readonly LocalFilesManager _localFilesManager = new LocalFilesManager();
        private readonly GoogleDriveManager _googleDriveManager = new GoogleDriveManager();

        public Dictionary<string, Cell> Cells
        {
            get => _cells;
            set => _cells = value;
        }

        public int RowNumber
        {
            get => _rowNumber;
            set => _rowNumber = value;
        }

        public int ColumnNumber
        {
            get => _columnNumber;
            set => _columnNumber = value;
        }

        public bool IsExpressionMode { get; set; } = false;

        public MainViewModel(int initialRows = 15, int initialColumns = 10)
        {
            _rowNumber = initialRows;
            _columnNumber = initialColumns;
        }

        public void AddRow()
        {
            _rowNumber++;
        }

        public void AddColumn()
        {
            _columnNumber++;
        }

        public void DeleteRow()
        {
            if (_rowNumber > 1)
            {
                int deletedRow = _rowNumber;
                _rowNumber--;

                for (int col = 1; col <= _columnNumber; col++)
                {
                    string colName = ((char)('A' + col - 1)).ToString();
                    string address = colName + deletedRow;

                    _tracker.ClearDependencies(address);
                    _cells.Remove(address);

                    foreach (string dep in _tracker.GetDependentCells(address))
                    {
                        if (_cells.ContainsKey(dep))
                        {
                            CalculateCell(_cells[dep]);
                        }
                    }
                }
            }
        }

        public void DeleteColumn()
        {
            if (_columnNumber > 1)
            {
                int deletedCol = _columnNumber;
                _columnNumber--;

                string colName = ((char)('A' + deletedCol - 1)).ToString();

                for (int row = 1; row <= _rowNumber; row++)
                {
                    string address = colName + row;

                    _tracker.ClearDependencies(address);
                    _cells.Remove(address);

                    foreach (string dep in _tracker.GetDependentCells(address))
                    {
                        if (_cells.ContainsKey(dep))
                        {
                            CalculateCell(_cells[dep]);
                        }
                    }
                }
            }
        }

        public void SwitchMode()
        {
            IsExpressionMode = !IsExpressionMode;
        }

        public void SaveLocal(string path)
        {
            _localFilesManager.SaveToFile(path, _cells);
        }

        public void LoadLocal(string path)
        {
            _cells = _localFilesManager.LoadFromFile(path);
            foreach (var cell in _cells.Values)
            {
                CalculateCell(cell);
            }
        }

        public bool SaveToGoogleDrive(string localPath)
        {
            if (_googleDriveManager.Authenticate())
            {
                _localFilesManager.SaveToFile(localPath, _cells);
                _googleDriveManager.UploadFile(localPath);
                return true;
            }
            return false;
        }

        public Dictionary<string, string> GetGoogleDriveFiles()
        {
            if (_googleDriveManager.Authenticate())
            {
                return _googleDriveManager.GetFilesList();
            }
            return new Dictionary<string, string>();
        }

        public bool LoadFromGoogleDrive(string fileId)
        {
            if (_googleDriveManager.Authenticate())
            {
                string downloadedPath = _googleDriveManager.DownloadFile(fileId);
                LoadLocal(downloadedPath);
                return true;
            }
            return false;
        }

        public void CalculateCell(Cell cell)
        {
            UpdateDependencies(cell);

            if (_tracker.CheckCycles(cell.Address))
            {
                cell.HasError = true;
                cell.ErrorMessage = "Циклічне посилання!";
                return;
            }

            cell.HasError = false;
            cell.ErrorMessage = "";

            try
            {
                if (!string.IsNullOrWhiteSpace(cell.Expression))
                {
                    var matches = Regex.Matches(cell.Expression, @"[A-Za-z]+[0-9]+");
                    foreach (Match match in matches)
                    {
                        string refAddress = match.Value.ToUpper();
                        if (_cells.ContainsKey(refAddress) && _cells[refAddress].HasError)
                        {
                            throw new Exception(_cells[refAddress].ErrorMessage);
                        }
                    }
                }
                var expr = new Expressions(GetCurrentVariables());
                cell.CalculatedValue = expr.EvaluateExpression(cell.Expression);
            }
            catch (ArgumentException ex)
            {
                cell.HasError = true;
                cell.ErrorMessage = ex.Message;
            }
            catch (Exception ex)
            {
                cell.HasError = true;
                cell.ErrorMessage = ex.Message;
            }
            foreach (var depAddress in _tracker.GetDependentCells(cell.Address))
            {
                CalculateCell(GetCell(depAddress));
            }
        }

        public Cell GetCell(string address)
        {
            if (!_cells.ContainsKey(address))
            {
                _cells[address] = new Cell { Address = address };
            }
            return _cells[address];
        }

        private Dictionary<string, object> GetCurrentVariables()
        {
            var vars = new Dictionary<string, object>();
            foreach (string address in _cells.Keys)
            {
                if (_cells[address].CalculatedValue != null)
                {
                    vars[address] = _cells[address].CalculatedValue;
                }
            }
            return vars;
        }

        private void UpdateDependencies(Cell cell)
        {
            _tracker.ClearDependencies(cell.Address);
            if (string.IsNullOrWhiteSpace(cell.Expression))
            {
                return;
            }

            var matches = Regex.Matches(cell.Expression, @"[A-Za-z]+[0-9]+");
            foreach (Match match in matches)
            {
                _tracker.AddDependency(cell.Address, match.Value.ToUpper());
            }
        }
    }
}
