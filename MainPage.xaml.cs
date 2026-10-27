using Microsoft.Maui.Controls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Spreadsheet_lab1.Core;

namespace Spreadsheet_lab1
{
    public partial class MainPage : ContentPage
    {
        private MainViewModel _viewModel = new MainViewModel();
        private Dictionary<string, Entry> _entries = new Dictionary<string, Entry>();
        private string _selectedAddress = "A1";

        public MainPage()
        {
            InitializeComponent();
            CreateGrid();
        }

        private void CreateGrid()
        {
            grid.Children.Clear();
            grid.RowDefinitions.Clear();
            grid.ColumnDefinitions.Clear();
            _entries.Clear();

            grid.RowDefinitions.Add(new RowDefinition { Height = 30 });

            for (int col = 0; col <= _viewModel.ColumnNumber; col++)
            {
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = 80 });
                if (col > 0)
                {
                    var label = new Label { Text = GetColumnName(col), HorizontalOptions = LayoutOptions.Center };
                    Grid.SetRow(label, 0);
                    Grid.SetColumn(label, col);
                    grid.Children.Add(label);
                }
            }

            for (int row = 0; row < _viewModel.RowNumber; row++)
            {
                grid.RowDefinitions.Add(new RowDefinition { Height = 40 });
                var label = new Label
                {
                    Text = (row + 1).ToString(),
                    VerticalOptions = LayoutOptions.Center,
                    HorizontalOptions = LayoutOptions.Center,
                    HorizontalTextAlignment = TextAlignment.Center
                };
                Grid.SetRow(label, row + 1);
                Grid.SetColumn(label, 0);
                grid.Children.Add(label);

                for (int col = 0; col < _viewModel.ColumnNumber; col++)
                {
                    string address = $"{GetColumnName(col + 1)}{row + 1}";
                    var cell = _viewModel.GetCell(address);
                    var entry = new Entry { Text = GetCellDisplayText(cell) };

                    entry.Focused += (sender, eventArgs) =>
                    {
                        _selectedAddress = address;
                        addressLabel.Text = address;
                        string expr = _viewModel.GetCell(address).Expression;
                        entry.Text = expr;
                        textInput.Text = expr;
                    };

                    entry.Completed += (sender, eventArgs) =>
                    {
                        var match = System.Text.RegularExpressions.Regex.Match(address, @"^([A-Za-z]+)([0-9]+)$");
                        if (match.Success)
                        {
                            string colPart = match.Groups[1].Value;
                            int rowPart = int.Parse(match.Groups[2].Value);
                            int nextRow = rowPart + 1;
                            if (nextRow <= _viewModel.RowNumber)
                            {
                                string nextAddress = colPart + nextRow;
                                if (_entries.ContainsKey(nextAddress))
                                {
                                    _entries[nextAddress].Focus();
                                }
                            }
                        }
                    };

                    entry.Unfocused += (sender, eventArgs) =>
                    {
                        var curCell = _viewModel.GetCell(address);
                        if (entry.Text == null) { curCell.Expression = ""; }
                        else { curCell.Expression = entry.Text; }

                        if (string.IsNullOrWhiteSpace(curCell.Expression)) { curCell.HasError = false; }
                        else { _viewModel.CalculateCell(curCell); }

                        UpdateAllEntries();

                        if (curCell.HasError)
                        {
                            DisplayAlert("Помилка", curCell.ErrorMessage, "OK");
                        }
                    };

                    Grid.SetRow(entry, row + 1);
                    Grid.SetColumn(entry, col + 1);
                    grid.Children.Add(entry);
                    _entries[address] = entry;
                }
            }
        }

        private string GetCellDisplayText(Core.Cell cell)
        {
            if (string.IsNullOrWhiteSpace(cell.Expression)) { return ""; }
            if (_viewModel.IsExpressionMode) { return cell.Expression; }

            if (cell.HasError) { return "Помилка"; }

            if (cell.CalculatedValue != null)
            {
                return cell.CalculatedValue.ToString();
            }
            return "";
        }

        private void UpdateAllEntries()
        {
            foreach (string address in _entries.Keys)
            {
                var cell = _viewModel.GetCell(address);
                _entries[address].Text = GetCellDisplayText(cell);
            }
        }

        private string GetColumnName(int colIndex)
        {
            int dividend = colIndex;
            string columnName = string.Empty;
            while (dividend > 0)
            {
                int modulo = (dividend - 1) % 26;
                columnName = Convert.ToChar(65 + modulo) + columnName;
                dividend = (dividend - modulo) / 26;
            }
            return columnName;
        }

        private void CalculateButton_Clicked(object sender, EventArgs e)
        {
            var cell = _viewModel.GetCell(_selectedAddress);
            if (textInput.Text == null) { cell.Expression = ""; }
            else { cell.Expression = textInput.Text; }

            if (string.IsNullOrWhiteSpace(cell.Expression)) { cell.HasError = false; }
            else { _viewModel.CalculateCell(cell); }

            UpdateAllEntries();

            if (cell.HasError)
            {
                DisplayAlert("Помилка", cell.ErrorMessage, "OK");
            }
            else
            {
                var match = System.Text.RegularExpressions.Regex.Match(_selectedAddress, @"^([A-Za-z]+)([0-9]+)$");
                if (match.Success)
                {
                    string colPart = match.Groups[1].Value;
                    int rowPart = int.Parse(match.Groups[2].Value);
                    int nextRow = rowPart + 1;
                    if (nextRow <= _viewModel.RowNumber)
                    {
                        string nextAddress = colPart + nextRow;
                        _selectedAddress = nextAddress;
                        addressLabel.Text = nextAddress;
                        var nextCell = _viewModel.GetCell(nextAddress);
                        textInput.Text = nextCell.Expression;
                        textInput.Focus();
                        if (textInput.Text != null)
                        {
                            textInput.CursorPosition = textInput.Text.Length;
                        }
                    }
                }
            }
        }

        private void SwitchModeButton_Clicked(object sender, EventArgs e)
        {
            _viewModel.SwitchMode();
            if (_viewModel.IsExpressionMode)
            {
                modeBtn.Text = "Режим: ВИРАЗ";
            }
            else
            {
                modeBtn.Text = "Режим: ЗНАЧЕННЯ";
            }
            UpdateAllEntries();
        }

        private void AddRowButton_Clicked(object sender, EventArgs e)
        {
            _viewModel.AddRow();
            CreateGrid();
        }

        private void AddColumnButton_Clicked(object sender, EventArgs e)
        {
            _viewModel.AddColumn();
            CreateGrid();
        }

        private void DeleteRowButton_Clicked(object sender, EventArgs e)
        {
            _viewModel.DeleteRow();
            CreateGrid();
        }

        private void DeleteColumnButton_Clicked(object sender, EventArgs e)
        {
            _viewModel.DeleteColumn();
            CreateGrid();
        }

        private async void SaveButton_Clicked(object sender, EventArgs e)
        {
            var savePicker = new Windows.Storage.Pickers.FileSavePicker();
            savePicker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.Downloads;
            savePicker.FileTypeChoices.Add("JSON файл таблиці", new List<string>() { ".json" });
            savePicker.SuggestedFileName = "table";
            IntPtr hwnd = ((MauiWinUIWindow)App.Current.Windows[0].Handler.PlatformView).WindowHandle;
            WinRT.Interop.InitializeWithWindow.Initialize(savePicker, hwnd);
            Windows.Storage.StorageFile file = await savePicker.PickSaveFileAsync();
            if (file != null)
            {
                _viewModel.SaveLocal(file.Path);
                await DisplayAlert("Успіх", "Таблиця збережена у файл:\n" + file.Path, "OK");
            }
        }

        private async void LoadButton_Clicked(object sender, EventArgs e)
        {
            try
            {
                FileResult result = await FilePicker.Default.PickAsync();
                if (result != null)
                {
                    _viewModel.LoadLocal(result.FullPath);
                    CreateGrid();
                    await DisplayAlert("Успіх", "Таблиця завантажена з файлу:\n" + result.FullPath, "OK");
                }
            }
            catch (Exception)
            {
                await DisplayAlert("Помилка", "Обраний файл пошкоджений або не є таблицею Kruhlexcel!", "OK");
            }
        }

        private async void SaveGoogle_Clicked(object sender, EventArgs e)
        {
            try
            {
                string fileName = await DisplayPromptAsync(
                    "Google Drive",
                    "Введіть назву файлу для збереження на Google Drive:",
                    "Зберегти",
                    "Скасувати",
                    "table.json");

                if (string.IsNullOrWhiteSpace(fileName))
                {
                    return;
                }

                if (!fileName.EndsWith(".json"))
                {
                    fileName = fileName + ".json";
                }

                string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), fileName);
                bool saved = await Task.Run(() => _viewModel.SaveToGoogleDrive(path));
                if (saved)
                {
                    await DisplayAlert("Google Drive", "Файл '" + fileName + "' успішно збережено на ваш Google Drive!", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Помилка Google Drive", ex.Message, "OK");
            }
        }

        private async void LoadGoogle_Clicked(object sender, EventArgs e)
        {
            try
            {
                Dictionary<string, string> files = await Task.Run(() => _viewModel.GetGoogleDriveFiles());
                if (files.Count == 0)
                {
                    await DisplayAlert("Google Drive", "На вашому Google Drive ще немає збережених файлів .json", "OK");
                    return;
                }

                string[] fileNames = files.Keys.ToArray();
                string selectedName = await DisplayActionSheet(
                    "Оберіть файл з вашого Google Drive:",
                    "Скасувати",
                    null,
                    fileNames);

                if (string.IsNullOrWhiteSpace(selectedName) || selectedName == "Скасувати")
                {
                    return;
                }

                if (files.ContainsKey(selectedName))
                {
                    string fileId = files[selectedName];
                    bool loaded = await Task.Run(() => _viewModel.LoadFromGoogleDrive(fileId));
                    if (loaded)
                    {
                        CreateGrid();
                        await DisplayAlert("Google Drive", "Файл '" + selectedName + "' успішно завантажено з Google Drive!", "OK");
                    }
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Помилка Google Drive", ex.Message, "OK");
            }
        }

        private async void HelpButton_Clicked(object sender, EventArgs e)
        {
            await DisplayAlert(
                "Довідка",
                "Лабораторна робота 1*" +
                "\nВаріант 29" +
                "\nКруглєня Юлія К-28" +
                "\nДозволені операції: +, -, *, /, mod, div, mmax, mmin, <=, >=, <>, not" +
                "\nРезультат виразу: логічний (True / False) або арифметичний",
                "OK");
        }

        private async void ExitButton_Clicked(object sender, EventArgs e)
        {
            if (await DisplayAlert("Підтвердження", "Ви точно бажаєте вийти?", "Так", "Ні"))
            {
                System.Environment.Exit(0);
            }
        }
    }
}
