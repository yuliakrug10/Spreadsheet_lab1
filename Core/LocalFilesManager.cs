using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace Spreadsheet_lab1.Core
{
    public class LocalFilesManager
    {
        public void SaveToFile(string filePath, Dictionary<string, Cell> cells)
        {
            string json = JsonSerializer.Serialize(cells);
            File.WriteAllText(filePath, json);
        }

        public Dictionary<string, Cell> LoadFromFile(string filePath)
        {
            if (!File.Exists(filePath))
            {
                return new Dictionary<string, Cell>();
            }
            try
            {
                string json = File.ReadAllText(filePath);
                var result = JsonSerializer.Deserialize<Dictionary<string, Cell>>(json);
                if (result != null)
                {
                    return result;
                }
            }
            catch
            {
                return new Dictionary<string, Cell>();
            }
            return new Dictionary<string, Cell>();
        }
    }
}
