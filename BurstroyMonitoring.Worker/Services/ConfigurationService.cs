using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace BurstroyMonitoring.Worker.Services;



/// <summary>
/// Сервис для управления конфигурацией
/// </summary>
public class ConfigurationService
{
    private readonly IConfiguration _configuration;
    private readonly DatabaseService _dbService;
    private readonly ILogger<ConfigurationService> _logger;
    
    // Кэш конфигурации
    private Dictionary<string, string> _configCache = new();
    private DateTime _lastConfigRefresh = DateTime.MinValue;
    private readonly object _lock = new();

    public ConfigurationService(
        IConfiguration configuration,
        DatabaseService dbService,
        ILogger<ConfigurationService> logger)
    {
        _configuration = configuration;
        _dbService = dbService;
        _logger = logger;
    }

    /// <summary>
    /// Обновление конфигурации из базы данных
    /// </summary>
    public async Task RefreshConfigurationAsync()
    {
        var refreshInterval = GetRefreshIntervalSeconds();
        
        if ((DateTime.UtcNow - _lastConfigRefresh).TotalSeconds < refreshInterval)
            return;

        lock (_lock)
        {
            if ((DateTime.UtcNow - _lastConfigRefresh).TotalSeconds < refreshInterval)
                return;
        }

        try
        {
            var configs = await _dbService.GetConfigurationAsync();
            
            lock (_lock)
            {
                _configCache.Clear();
                foreach (var config in configs)
                {
                    _configCache[config.Key] = config.Value;
                }
                _lastConfigRefresh = DateTime.UtcNow;
                
                _logger.LogInformation("Configuration refreshed. Loaded {count} items", configs.Count);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing configuration");
        }
    }

    /// <summary>
    /// Получение значения конфигурации с приоритетом:
    /// 1. База данных
    /// 2. appsettings.json
    /// </summary>
    public string GetConfigValue(string key, string defaultValue = "")
    {
        // Попытка получить из кэша базы данных
        if (_configCache.TryGetValue(key, out var cachedValue))
            return cachedValue;

        // Попытка получить из appsettings.json
        var appSettingsValue = _configuration[key];
        if (!string.IsNullOrEmpty(appSettingsValue))
            return appSettingsValue;

        return defaultValue;
    }

    /// <summary>
    /// Проверка, нужно ли сохранять тело ответа для датчика
    /// </summary>
    public bool ShouldSaveResponseBody(int sensorId)
    {
        // Сначала проверяем конкретную настройку для датчика
        var sensorSpecificKey = $"SaveResponseBody.Sensor.{sensorId}";
        var sensorSpecificValue = GetConfigValue(sensorSpecificKey);
        
        if (!string.IsNullOrEmpty(sensorSpecificValue))
            return bool.Parse(sensorSpecificValue);

        // Используем значение по умолчанию
        var defaultValue = GetConfigValue("SaveResponseBody.Default", "true");
        return bool.Parse(defaultValue);
    }

    /// <summary>
    /// Получение максимального количества параллельных задач
    /// </summary>
    public int GetMaxConcurrentTasks()
    {
        var value = GetConfigValue("Polling.MaxConcurrentTasks", "100");
        return int.TryParse(value, out int result) ? result : 100;
    }

    /// <summary>
    /// Получение количества повторных попыток
    /// </summary>
    public int GetRetryCount()
    {
        var value = GetConfigValue("Polling.RetryCount", "3");
        return int.TryParse(value, out int result) ? result : 3;
    }

    /// <summary>
    /// Получение задержки между повторными попытками
    /// </summary>
    public int GetRetryDelayMs()
    {
        var value = GetConfigValue("Polling.RetryDelayMs", "1000");
        return int.TryParse(value, out int result) ? result : 1000;
    }

    /// <summary>
    /// Получение интервала обновления конфигурации
    /// </summary>
    private int GetRefreshIntervalSeconds()
    {
        var value = GetConfigValue("Configuration.RefreshIntervalSeconds", "60");
        return int.TryParse(value, out int result) ? result : 60;
    }

    //Добавить в базу!
    public int GetHttpTimeoutSeconds()
    {
        var value = GetConfigValue("Polling.HttpTimeoutSeconds", "30");
        return int.TryParse(value, out int result) ? result : 30;
    }
}