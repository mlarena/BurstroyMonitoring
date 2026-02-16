using OfficeOpenXml;
using CsvHelper;
using System.Globalization;
using System.Text;
using System.Reflection;

namespace BurstroyMonitoring.TCM.Services
{
    public class ExportService : IExportService
    {
        public byte[] ExportToExcel<T>(IEnumerable<T> data, List<string> selectedFields, List<string>? displayNames = null)
        {
            return ExportToExcelInternal(data, selectedFields, displayNames);
        }

        public byte[] ExportToExcel<T>(IEnumerable<T> data, List<string> selectedFields)
        {
            return ExportToExcelInternal(data, selectedFields, null);
        }

        public byte[] ExportToCsv<T>(IEnumerable<T> data, List<string> selectedFields, List<string>? displayNames = null)
        {
            return ExportToCsvInternal(data, selectedFields, displayNames);
        }

        public byte[] ExportToCsv<T>(IEnumerable<T> data, List<string> selectedFields)
        {
            return ExportToCsvInternal(data, selectedFields, null);
        }

        private byte[] ExportToExcelInternal<T>(IEnumerable<T> data, List<string> selectedFields, List<string>? displayNames)
        {
           
            
            var headers = displayNames ?? selectedFields;
            
            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Данные DOV");
            
            // Заголовки
            for (int i = 0; i < headers.Count; i++)
            {
                worksheet.Cells[1, i + 1].Value = headers[i];
                worksheet.Cells[1, i + 1].Style.Font.Bold = true;
                worksheet.Cells[1, i + 1].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                worksheet.Cells[1, i + 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightBlue);
                worksheet.Column(i + 1).Width = 20;
            }
            
            // Данные
            var properties = typeof(T).GetProperties();
            int row = 2;
            
            foreach (var item in data)
            {
                for (int i = 0; i < selectedFields.Count; i++)
                {
                    var property = properties.FirstOrDefault(p => 
                        p.Name.Equals(selectedFields[i], StringComparison.OrdinalIgnoreCase));
                    
                    if (property != null)
                    {
                        var value = property.GetValue(item);
                        worksheet.Cells[row, i + 1].Value = FormatValue(value);
                    }
                }
                row++;
            }
            
            // Автофильтр
            if (data.Any())
            {
                worksheet.Cells[1, 1, 1, selectedFields.Count].AutoFilter = true;
            }
            
            return package.GetAsByteArray();
        }

        private byte[] ExportToCsvInternal<T>(IEnumerable<T> data, List<string> selectedFields, List<string>? displayNames)
        {
            var headers = displayNames ?? selectedFields;
            
            using var memoryStream = new MemoryStream();
            using var writer = new StreamWriter(memoryStream, new UTF8Encoding(true));
            
            // Заголовки с разделителем-точкой с запятой (стандарт для русской локали)
            writer.WriteLine(string.Join(";", headers));
            
            // Данные
            var properties = typeof(T).GetProperties();
            
            foreach (var item in data)
            {
                var values = new List<string>();
                foreach (var field in selectedFields)
                {
                    var property = properties.FirstOrDefault(p => 
                        p.Name.Equals(field, StringComparison.OrdinalIgnoreCase));
                    
                    var value = property?.GetValue(item);
                    values.Add(FormatCsvValue(value));
                }
                writer.WriteLine(string.Join(";", values));
            }
            
            writer.Flush();
            return memoryStream.ToArray();
        }

        private object? FormatValue(object? value)
        {
            if (value == null) return null;
            
            if (value is DateTime dateTime)
            {
                return dateTime.ToString("dd.MM.yyyy HH:mm:ss");
            }
            
            if (value is bool boolValue)
            {
                return boolValue ? "Да" : "Нет";
            }
            
            if (value is int intValue && (value.GetType().Name.Contains("Flag") || value.GetType().Name.Contains("Status")))
            {
                return intValue == 1 ? "Да" : "Нет";
            }
            
            return value;
        }

        private string FormatCsvValue(object? value)
        {
            if (value == null) return "";
            
            var formatted = FormatValue(value);
            var str = formatted?.ToString() ?? "";
            
            // Экранируем кавычки для CSV
            if (str.Contains(";") || str.Contains("\"") || str.Contains("\n") || str.Contains("\r"))
            {
                str = "\"" + str.Replace("\"", "\"\"") + "\"";
            }
            
            return str;
        }
    }
}