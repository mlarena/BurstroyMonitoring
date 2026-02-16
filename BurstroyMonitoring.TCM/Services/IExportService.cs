namespace BurstroyMonitoring.TCM.Services
{
    public interface IExportService
    {
        byte[] ExportToExcel<T>(IEnumerable<T> data, List<string> selectedFields, List<string>? displayNames = null);
        byte[] ExportToCsv<T>(IEnumerable<T> data, List<string> selectedFields, List<string>? displayNames = null);
        
        byte[] ExportToExcel<T>(IEnumerable<T> data, List<string> selectedFields);
        byte[] ExportToCsv<T>(IEnumerable<T> data, List<string> selectedFields);
    }
}