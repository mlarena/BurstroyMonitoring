using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using BurstroyMonitoring.Worker.Services;

namespace BurstroyMonitoring.Worker;

/// <summary>
/// Основной рабочий процесс приложения
/// Управляет опросом датчиков по расписанию
/// </summary>
public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly ConfigurationService _configService;
    private readonly DatabaseService _dbService;

    // Словарь для хранения времени следующего опроса каждого датчика
    private Dictionary<int, DateTime> _nextPollTimes = new();
    private readonly object _lock = new();
    private DateTime _lastStatusLog = DateTime.UtcNow;

    public Worker(
        ILogger<Worker> logger,
        IServiceProvider serviceProvider,
        ConfigurationService configService,
        DatabaseService dbService)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _configService = configService;
        _dbService = dbService;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Worker started at: {time}", DateTimeOffset.Now);

        // Основной цикл работы
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // Обновление конфигурации
                await _configService.RefreshConfigurationAsync();

                // Загрузка активных датчиков
                var sensors = await _dbService.GetActiveSensorsAsync();
                
                // Обновление расписания опроса
                UpdatePollSchedule(sensors);

                // Получение датчиков для опроса в текущий момент
                var sensorsToPoll = GetSensorsToPoll();

                if (sensorsToPoll.Any())
                {
                    _logger.LogDebug("Polling {count} sensors", sensorsToPoll.Count);
                    
                    // Создаем scope для каждого цикла опроса
                    using var scope = _serviceProvider.CreateScope();
                    var pollingService = scope.ServiceProvider.GetRequiredService<SensorPollingService>();
                    
                    // Параллельный опрос датчиков
                    await pollingService.PollSensorsAsync(sensorsToPoll, stoppingToken);
                }

                // Логирование статуса каждые 60 секунд
                if ((DateTime.UtcNow - _lastStatusLog).TotalSeconds >= 60)
                {
                    _logger.LogInformation("Worker status: {activeSensors} active sensors, {scheduled} in schedule", 
                        sensors.Count, _nextPollTimes.Count);
                    _lastStatusLog = DateTime.UtcNow;
                }

                // Ожидание до следующей проверки
                await Task.Delay(1000, stoppingToken); // Проверка каждую секунду
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in worker main loop");
                await Task.Delay(5000, stoppingToken); // Пауза при ошибке
            }
        }
    }

    /// <summary>
    /// Обновление расписания опроса датчиков
    /// </summary>
    private void UpdatePollSchedule(List<BurstroyMonitoring.Data.Models.Sensor> sensors)
    {
        lock (_lock)
        {
            foreach (var sensor in sensors)
            {
                if (!_nextPollTimes.ContainsKey(sensor.Id))
                {
                    // Первый опрос - выполнить немедленно
                    _nextPollTimes[sensor.Id] = DateTime.UtcNow;
                }
            }

            // Удаление неактивных датчиков из расписания
            var activeSensorIds = sensors.Select(s => s.Id).ToHashSet();
            var inactiveSensorIds = _nextPollTimes.Keys.Where(id => !activeSensorIds.Contains(id)).ToList();
            
            foreach (var id in inactiveSensorIds)
            {
                _nextPollTimes.Remove(id);
            }
        }
    }

    /// <summary>
    /// Получение списка датчиков для опроса
    /// </summary>
    private List<BurstroyMonitoring.Data.Models.Sensor> GetSensorsToPoll()
    {
        lock (_lock)
        {
            var now = DateTime.UtcNow;
            var sensorIdsToPoll = _nextPollTimes
                .Where(kvp => kvp.Value <= now) // Находим датчики, у которых время опроса наступило
                .Select(kvp => kvp.Key)
                .ToList();

            // Обновление времени следующего опроса
            foreach (var sensorId in sensorIdsToPoll)
            {
                var sensor = _dbService.GetSensorById(sensorId);
                if (sensor != null)
                {
                    _nextPollTimes[sensorId] = now.AddSeconds(sensor.CheckIntervalSeconds);
                }
            }

            return _dbService.GetSensorsByIds(sensorIdsToPoll);
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Worker stopping at: {time}", DateTimeOffset.Now);
        await base.StopAsync(cancellationToken);
    }
}