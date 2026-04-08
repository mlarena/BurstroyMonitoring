using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace BurstroyMonitoring.Worker.Services;

public class ConfigurationService
{
    private readonly IConfiguration _configuration;
    private readonly DatabaseService _dbService;
    private readonly ILogger<ConfigurationService> _logger;
    
    private System.Collections.Generic.Dictionary<string, string> _configCache = new System.Collections.Generic.Dictionary<string, string>();
    private System.DateTime _lastConfigRefresh = System.DateTime.MinValue;
    private readonly object _lock = new object();

    public ConfigurationService(
        IConfiguration configuration,
        DatabaseService dbService,
        ILogger<ConfigurationService> logger)
    {
        _configuration = configuration;
        _dbService = dbService;
        _logger = logger;
    }

    public async System.Threading.Tasks.Task RefreshConfigurationAsync()
    {
        var refreshInterval = GetRefreshIntervalSeconds();
        if ((System.DateTime.UtcNow - _lastConfigRefresh).TotalSeconds < refreshInterval)
            return;

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
                _lastConfigRefresh = System.DateTime.UtcNow;
                _logger.LogInformation("Configuration refreshed. Loaded {count} items", configs.Count);
            }
        }
        catch (System.Exception ex)
        {
            _logger.LogError(ex, "Error refreshing configuration");
        }
    }

    public string GetConfigValue(string key, string defaultValue = "")
    {
        if (_configCache.TryGetValue(key, out var cachedValue))
            return cachedValue;

        var appSettingsValue = _configuration[key];
        if (!string.IsNullOrEmpty(appSettingsValue))
            return appSettingsValue;

        return defaultValue;
    }

    public int GetMaxConcurrentTasks()
    {
        var value = GetConfigValue("Polling.MaxConcurrentTasks", "100");
        return int.TryParse(value, out int result) ? result : 100;
    }

    public int GetRetryCount()
    {
        var value = GetConfigValue("Polling.RetryCount", "3");
        return int.TryParse(value, out int result) ? result : 3;
    }

    public int GetRetryDelayMs()
    {
        var value = GetConfigValue("Polling.RetryDelayMs", "1000");
        return int.TryParse(value, out int result) ? result : 1000;
    }

    private int GetRefreshIntervalSeconds()
    {
        var value = GetConfigValue("Configuration.RefreshIntervalSeconds", "60");
        return int.TryParse(value, out int result) ? result : 60;
    }

    public int GetHttpTimeoutSeconds()
    {
        var value = GetConfigValue("Polling.HttpTimeoutSeconds", "30");
        return int.TryParse(value, out int result) ? result : 30;
    }
}
