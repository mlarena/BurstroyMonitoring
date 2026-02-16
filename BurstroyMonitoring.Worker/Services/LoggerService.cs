using Microsoft.Extensions.Logging;
using System.Text.Json;
using BurstroyMonitoring.Data.Models;

namespace BurstroyMonitoring.Worker.Services;

/// <summary>
/// Сервис для логирования ошибок
/// </summary>
public class LoggerService
{
    private readonly ILogger<LoggerService> _logger;
    private readonly DatabaseService _dbService;
    private static readonly object _fileLock = new object();
    private static readonly SemaphoreSlim _dbSemaphore = new SemaphoreSlim(1, 1);

    public LoggerService(
        ILogger<LoggerService> logger,
        DatabaseService dbService)
    {
        _logger = logger;
        _dbService = dbService;
    }

    /// <summary>
    /// Логирование ошибки доступа в базу данных
    /// </summary>
    public async Task LogDatabaseErrorAsync(
        Sensor sensor,
        string errorLevel,
        string errorMessage,
        Exception? exception = null)
    {
        try
        {
            await _dbSemaphore.WaitAsync();
            
            try
            {
                JsonDocument? additionalData = null;
                
                if (exception != null)
                {
                    var jsonString = JsonSerializer.Serialize(new
                    {
                        ExceptionMessage = exception.Message,
                        ExceptionType = exception.GetType().Name,
                        InnerException = exception.InnerException?.Message,
                        Url = sensor.Url,
                        SensorSerial = sensor.SerialNumber,
                        PostName = sensor.MonitoringPost?.Name
                    });
                    additionalData = JsonDocument.Parse(jsonString);
                }

                var error = new SensorError
                {
                    SensorId = sensor.Id,
                    ErrorLevel = errorLevel,
                    ErrorMessage = errorMessage,
                    StackTrace = exception?.StackTrace,
                    ErrorSource = exception?.Source,
                    ExceptionType = exception?.GetType().Name,
                    CreatedAt = DateTime.UtcNow,
                    AdditionalData = additionalData
                };

                await _dbService.SaveSensorErrorAsync(error);
            }
            finally
            {
                _dbSemaphore.Release();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error logging to database for sensor {SerialNumber}", sensor.SerialNumber);
            
            if (exception != null)
            {
                await LogToFileAsync(exception, $"Failed to log database error for sensor {sensor.SerialNumber}", sensor.Url);
            }
        }
    }

    /// <summary>
    /// Логирование в файл ошибок (упрощенное для HttpRequestException)
    /// </summary>
    public async Task LogToFileAsync(Exception exception, string message, string url)
    {
        try
        {
            string logEntry;
            
            if (exception is System.Net.Http.HttpRequestException httpEx)
            {
                logEntry = $"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss.fff}] ERROR: {message} ({url}) Exception: {httpEx.Message} ({url})\n";
            }
            else
            {
                logEntry = $"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss.fff}] ERROR: {message} ({url}) Exception: {exception.Message} Type: {exception.GetType().Name}\n";
            }

            var logDirectory = Path.Combine(Directory.GetCurrentDirectory(), "logs");
            if (!Directory.Exists(logDirectory))
                Directory.CreateDirectory(logDirectory);

            var logFile = Path.Combine(logDirectory, $"errors-{DateTime.UtcNow:yyyy-MM-dd}.log");
            
            lock (_fileLock)
            {
                try
                {
                    File.AppendAllText(logFile, logEntry);
                }
                catch (IOException)
                {
                    Thread.Sleep(10);
                    try
                    {
                        File.AppendAllText(logFile, logEntry);
                    }
                    catch (IOException)
                    {
                        var backupFile = Path.Combine(logDirectory, $"errors-backup-{DateTime.UtcNow:yyyy-MM-dd-HHmmss}.log");
                        File.AppendAllText(backupFile, logEntry);
                        _logger.LogWarning("Could not write to main error log, wrote to backup file: {backupFile}", backupFile);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to write to error log file");
        }
    }

    /// <summary>
    /// Логирование информационного сообщения в файл
    /// </summary>
    public async Task LogInfoToFileAsync(string message)
    {
        try
        {
            var logEntry = $"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss.fff}] INFO: {message}\n";

            var logDirectory = Path.Combine(Directory.GetCurrentDirectory(), "logs");
            if (!Directory.Exists(logDirectory))
                Directory.CreateDirectory(logDirectory);

            var logFile = Path.Combine(logDirectory, $"app-{DateTime.UtcNow:yyyy-MM-dd}.log");
            
            lock (_fileLock)
            {
                try
                {
                    File.AppendAllText(logFile, logEntry);
                }
                catch (IOException)
                {
                    Thread.Sleep(10);
                    File.AppendAllText(logFile, logEntry);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to write info to log file");
        }
    }

    /// <summary>
    /// Логирование ошибки доступа с обработкой параллелизма
    /// </summary>
    public async Task LogAccessErrorAsync(Sensor sensor, Exception exception, CancellationToken cancellationToken)
    {
        try
        {
            // Формируем информативное сообщение с серийным номером и именем поста
            string sensorInfo = $"sensor '{sensor.SerialNumber}'";
            string postInfo = sensor.MonitoringPost != null 
                ? $" on post '{sensor.MonitoringPost.Name}'" 
                : "";
            
            string fullMessage = $"Access error for {sensorInfo}{postInfo}";
            
            // Логирование ошибки в базу данных
            await LogDatabaseErrorAsync(sensor, "ACCESS_ERROR", fullMessage, exception);
            
            // Логирование в файл
            await LogToFileAsync(exception, fullMessage, sensor.Url);
            
            // Сообщение в консоль и основной лог
            if (exception is System.Net.Http.HttpRequestException httpEx)
            {
                _logger.LogError("Access error for {sensorInfo}{postInfo} ({url}): {message}", 
                    sensorInfo, postInfo, sensor.Url, httpEx.Message);
            }
            else
            {
                _logger.LogError("Access error for {sensorInfo}{postInfo} ({url}): {message} (Type: {exceptionType})", 
                    sensorInfo, postInfo, sensor.Url, exception.Message, exception.GetType().Name);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to log access error for sensor '{SerialNumber}'", sensor.SerialNumber);
        }
    }
}