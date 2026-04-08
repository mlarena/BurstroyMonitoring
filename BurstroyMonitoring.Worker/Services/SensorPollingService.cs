using System.Net;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using BurstroyMonitoring.Data;

namespace BurstroyMonitoring.Worker.Services;

/// <summary>
/// Сервис для опроса датчиков через HTTP
/// </summary>
public class SensorPollingService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly DatabaseService _dbService;
    private readonly ConfigurationService _configService;
    private readonly LoggerService _loggerService;
    private readonly ILogger<SensorPollingService> _logger;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly IServiceProvider _serviceProvider;

    public SensorPollingService(
        IHttpClientFactory httpClientFactory,
        DatabaseService dbService,
        ConfigurationService configService,
        LoggerService loggerService,
        ILogger<SensorPollingService> logger,
        IServiceProvider serviceProvider)
    {
        _httpClientFactory = httpClientFactory;
        _dbService = dbService;
        _configService = configService;
        _loggerService = loggerService;
        _logger = logger;
        _serviceProvider = serviceProvider;
        
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    }

    public async Task<(string? ResponseBody, int StatusCode, long ResponseTimeMs, bool IsSuccess, Exception? Exception)> PollSensorAsync(BurstroyMonitoring.Data.Models.Sensor sensor, CancellationToken cancellationToken)
    {
        var startTime = DateTime.UtcNow;
        HttpResponseMessage? response = null;
        string? responseBody = null;
        int statusCode = 0;
        long responseTimeMs = 0;
        bool isSuccess = false;
        Exception? lastException = null;

        try
        {
            var client = _httpClientFactory.CreateClient("SensorClient");
            
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(TimeSpan.FromSeconds(_configService.GetHttpTimeoutSeconds() + 5));
            
            for (int attempt = 1; attempt <= _configService.GetRetryCount(); attempt++)
            {
                try
                {
                    var requestStart = DateTime.UtcNow;
                    response = await client.GetAsync(sensor.Url, cts.Token);
                    responseTimeMs = (long)(DateTime.UtcNow - requestStart).TotalMilliseconds;
                    
                    statusCode = (int)response.StatusCode;
                    isSuccess = response.IsSuccessStatusCode;

                    if (response.IsSuccessStatusCode)
                    {
                        responseBody = await response.Content.ReadAsStringAsync(cts.Token);
                        break;
                    }
                    
                    if (attempt < _configService.GetRetryCount())
                    {
                        await Task.Delay(_configService.GetRetryDelayMs(), cts.Token);
                    }
                }
                catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException || cts.Token.IsCancellationRequested)
                {
                    statusCode = 408;
                    lastException = ex;
                    if (attempt == _configService.GetRetryCount()) break;
                    await Task.Delay(_configService.GetRetryDelayMs(), cts.Token);
                }
                catch (Exception ex)
                {
                    lastException = ex;
                    if (attempt == _configService.GetRetryCount()) break;
                    await Task.Delay(_configService.GetRetryDelayMs(), cts.Token);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning("Error polling sensor {sensorId}: {message}", sensor.Id, ex.Message);
            isSuccess = false;
            lastException = ex;
        }
        finally
        {
            response?.Dispose();
        }

        return (responseBody, statusCode, responseTimeMs, isSuccess, lastException);
    }


/// <summary>
/// Обработка ошибок доступа к конечной точке
/// </summary>
private async Task HandleAccessErrorAsync(
    BurstroyMonitoring.Data.Models.Sensor sensor, 
    Exception exception,
    CancellationToken cancellationToken)
{
    try
    {
        // Логирование ошибки в базу данных и файл
        await _loggerService.LogAccessErrorAsync(sensor, exception, cancellationToken);
        
        // Сохраняем неуспешный результат (если это ответ от сервера, а не ошибка сети)
        if (exception is HttpRequestException httpEx && httpEx.StatusCode.HasValue)
        {
            var result = new BurstroyMonitoring.Data.Models.SensorResults
            {
                SensorId = sensor.Id,
                CheckedAt = DateTime.UtcNow,
                StatusCode = (int)httpEx.StatusCode.Value,
                ResponseBody = null,
                ResponseTimeMs = 0,
                IsSuccess = false
            };

            await _dbService.SaveSensorResultAsync(result);
        }
    }
    catch (Exception logEx)
    {
        // Если не удалось залогировать ошибку, пишем простейшее сообщение
        _logger.LogError("Critical: Failed to process access error for sensor '{SerialNumber}' ({url}) " + logEx.Message.ToString(), 
            sensor.SerialNumber, GetShortUrl(sensor.Url));
    }
}

    /// <summary>
    /// Получение короткого URL для логов
    /// </summary>
    private string GetShortUrl(string fullUrl)
    {
        try
        {
            var uri = new Uri(fullUrl);
            return $"{uri.Host}:{uri.Port}";
        }
        catch
        {
            return fullUrl.Length > 50 ? fullUrl.Substring(0, 50) + "..." : fullUrl;
        }
    }

    /// <summary>
    /// Обработка успешного ответа от датчика
    /// </summary>
    private async Task ProcessSuccessfulResponseAsync(
        BurstroyMonitoring.Data.Models.Sensor sensor, 
        string responseBody, 
        int statusCode, 
        long responseTimeMs,
        DateTime checkedAt,
        CancellationToken cancellationToken)
    {
        try
        {
            // Сохранение результата в базу данных (всегда сохраняем тело ответа)
            JsonDocument? responseBodyJson = !string.IsNullOrEmpty(responseBody) 
                ? JsonDocument.Parse(responseBody) 
                : null;

            var result = new BurstroyMonitoring.Data.Models.SensorResults
            {
                SensorId = sensor.Id,
                CheckedAt = checkedAt,
                StatusCode = statusCode,
                ResponseBody = responseBodyJson,
                ResponseTimeMs = responseTimeMs,
                IsSuccess = true
            };

            await _dbService.SaveSensorResultAsync(result);

            // Десериализация JSON для дальнейшей обработки
            try
            {
                var jsonDocument = JsonDocument.Parse(responseBody);
                await ProcessSensorDataAsync(sensor, jsonDocument, cancellationToken);
            }
            catch (JsonException jsonEx)
            {
                _logger.LogError(jsonEx, "JSON deserialization error for sensor '{SerialNumber}'", sensor.SerialNumber);
                await _loggerService.LogDatabaseErrorAsync(sensor, "JSON_PARSE_ERROR", jsonEx.Message, jsonEx);
            }

            // Обновление времени последней активности
            await _dbService.UpdateSensorLastActivityAsync(sensor.Id);

            // Информативное логирование с серийным номером и именем поста
            string sensorInfo = $"sensor '{sensor.SerialNumber}'";
            string postInfo = sensor.MonitoringPost != null 
                ? $" on post '{sensor.MonitoringPost.Name}'" 
                : "";
            
            _logger.LogInformation("Successfully polled {sensorInfo}{postInfo} in {responseTimeMs}ms", 
                sensorInfo, postInfo, responseTimeMs);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing response for sensor '{SerialNumber}'", sensor.SerialNumber);
            await _loggerService.LogToFileAsync(ex, $"Error processing response for sensor {sensor.SerialNumber}", sensor.Url);
        }
    }

    /// <summary>
    /// Обработка данных датчика после десериализации
    /// </summary>
    private async Task ProcessSensorDataAsync(BurstroyMonitoring.Data.Models.Sensor sensor, JsonDocument jsonDocument, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Processing data for sensor {sensorId} of type {sensorType}", 
            sensor.Id, sensor.SensorType?.SensorTypeName);
        
        // Используем DataProcessingService
        using var scope = _serviceProvider.CreateScope();
        var dataProcessingService = scope.ServiceProvider.GetRequiredService<DataProcessingService>();
        await dataProcessingService.ProcessSensorDataAsync(sensor, jsonDocument, null, cancellationToken);
    }

    /// <summary>
    /// Обработка неуспешного ответа от работающего сервера
    /// </summary>
    private async Task HandleUnsuccessfulResponseAsync(
        BurstroyMonitoring.Data.Models.Sensor sensor, 
        int statusCode, 
        long responseTimeMs,
        string? responseBody,
        DateTime checkedAt,
        CancellationToken cancellationToken)
    {
        JsonDocument? responseBodyJson = !string.IsNullOrEmpty(responseBody) 
            ? JsonDocument.Parse(responseBody) 
            : null;

        var result = new BurstroyMonitoring.Data.Models.SensorResults
        {
            SensorId = sensor.Id,
            CheckedAt = checkedAt,
            StatusCode = statusCode,
            ResponseBody = responseBodyJson,
            ResponseTimeMs = responseTimeMs,
            IsSuccess = false
        };


        await _dbService.SaveSensorResultAsync(result);
        
        string sensorInfo = $"sensor '{sensor.SerialNumber}'";
        string postInfo = sensor.MonitoringPost != null 
            ? $" on post '{sensor.MonitoringPost.Name}'" 
            : "";
        
        _logger.LogWarning("Server error from {sensorInfo}{postInfo} ({url}): Status {statusCode}", 
            sensorInfo, postInfo, sensor.Url, statusCode);
    }
}