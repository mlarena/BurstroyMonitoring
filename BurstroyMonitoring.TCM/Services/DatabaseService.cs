using Npgsql;
using Microsoft.Extensions.Configuration;

namespace BurstroyMonitoring.TCM.Services
{
    public interface IDatabaseService
    {
        Task<double> GetDatabaseSizeInMB();
        string GetDatabaseName(); // Новый метод
    }

    public class DatabaseService : IDatabaseService
    {
        private readonly string _connectionString;
        private readonly string _databaseName;

        public DatabaseService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
            _databaseName = ExtractDatabaseName(_connectionString);
        }

        public async Task<double> GetDatabaseSizeInMB()
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                // SQL запрос для получения размера базы данных в байтах
                string query = @"
                    SELECT pg_database_size(datname) 
                    FROM pg_database 
                    WHERE datname = current_database()";

                using var command = new NpgsqlCommand(query, connection);
                var sizeInBytes = await command.ExecuteScalarAsync();

                if (sizeInBytes != null && sizeInBytes != DBNull.Value)
                {
                    // Конвертируем байты в мегабайты
                    return Convert.ToDouble(sizeInBytes) / (1024.0 * 1024.0);
                }

                return 0;
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка при получении размера БД: {ex.Message}", ex);
            }
        }

        public string GetDatabaseName()
        {
            return _databaseName;
        }

        private string ExtractDatabaseName(string connectionString)
        {
            try
            {
                var builder = new NpgsqlConnectionStringBuilder(connectionString);
                return builder.Database;
            }
            catch
            {
                // Ручной парсинг, если NpgsqlConnectionStringBuilder не сработал
                var parts = connectionString.Split(';');
                foreach (var part in parts)
                {
                    var keyValue = part.Trim().Split('=');
                    if (keyValue.Length == 2 && keyValue[0].Equals("Database", StringComparison.OrdinalIgnoreCase))
                    {
                        return keyValue[1];
                    }
                }
                return "Unknown";
            }
        }
    }
}