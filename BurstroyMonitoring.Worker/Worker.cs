using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using BurstroyMonitoring.Worker.Services;
using BurstroyMonitoring.Data.Models;

namespace BurstroyMonitoring.Worker;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly DatabaseService _dbService;
    private readonly DataProcessingService _dataProcessingService;

    public Worker(
        ILogger<Worker> logger,
        DatabaseService dbService,
        DataProcessingService dataProcessingService)
    {
        _logger = logger;
        _dbService = dbService;
        _dataProcessingService = dataProcessingService;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Worker started at: {time}", DateTimeOffset.Now);

        // Основной цикл работы воркера
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // 1. Получаем посты мониторинга, которые пора опрашивать согласно их интервалу
                var postsToPoll = await _dbService.GetPostsToPollAsync();
                
                if (postsToPoll.Count > 0)
                {
                    _logger.LogInformation("Found {count} posts to poll", postsToPoll.Count);
                    
                    // 2. Запускаем опрос каждого поста параллельно
                    var pollingTasks = postsToPoll.Select(post => 
                        _dataProcessingService.ProcessPostPollingAsync(post, stoppingToken));
                    
                    await Task.WhenAll(pollingTasks);
                }
                else
                {
                    _logger.LogDebug("No posts to poll at this time");
                }

                // 3. Опрос PUID датчиков (трафик). Выполняется отдельно по своему расписанию
                await ProcessPuidsAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in worker main loop");
            }

            // Пауза 10 секунд между проверками расписания в базе данных
            await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
        }
    }

    /// <summary>
    /// Логика поиска и запуска опроса для всех активных PUID-устройств.
    /// </summary>
    private async Task ProcessPuidsAsync(CancellationToken stoppingToken)
    {
        try
        {
            // Выбираем PUID, у которых подошло время опроса (LastActivityUTC + IntervalSeconds <= Now)
            var puidsToPoll = await _dbService.GetPuidsToPollAsync();
            if (puidsToPoll.Count == 0) return;

            _logger.LogInformation("Found {count} PUIDs to poll", puidsToPoll.Count);

            // Запускаем опрос каждого PUID параллельно
            var puidTasks = puidsToPoll.Select(puid => 
                _dataProcessingService.ProcessPuidPollingAsync(puid, stoppingToken));

            await Task.WhenAll(puidTasks);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing PUIDs");
        }
    }
}
