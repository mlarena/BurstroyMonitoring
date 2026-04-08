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

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // 1. Получаем посты, которые пора опрашивать
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
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in worker main loop");
            }

            // Пауза между проверками расписания
            await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
        }
    }
}
