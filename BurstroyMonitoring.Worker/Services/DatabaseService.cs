using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using BurstroyMonitoring.Data.Models;
using BurstroyMonitoring.Data;
using Microsoft.Extensions.DependencyInjection;

namespace BurstroyMonitoring.Worker.Services;

/// <summary>
/// Сервис для работы с базой данных.
/// Обеспечивает получение конфигурации, списка датчиков и сохранение результатов измерений.
/// </summary>
public class DatabaseService
{
    private readonly ILogger<DatabaseService> _logger;
    private readonly IServiceScopeFactory _scopeFactory;

    public DatabaseService(
        ILogger<DatabaseService> logger,
        IServiceScopeFactory scopeFactory)
    {
        _logger = logger;
        _scopeFactory = scopeFactory;
    }

    /// <summary>
    /// Получение списка активных постов, которые пора опрашивать.
    /// </summary>
    public async Task<List<MonitoringPost>> GetPostsToPollAsync()
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        try
        {
            var now = DateTime.UtcNow;
            return await context.MonitoringPosts
                .Include(p => p.Sensors.Where(s => s.IsActive))
                    .ThenInclude(s => s.SensorType)
                .Where(p => p.IsActive && 
                           (p.LastPolledAt == null || 
                            p.LastPolledAt.Value.AddSeconds(p.PollingIntervalSeconds) <= now))
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting posts to poll");
            return new List<MonitoringPost>();
        }
    }

    /// <summary>
    /// Создание новой сессии опроса.
    /// </summary>
    public async Task<Guid> StartPollingSessionAsync(int postId, int totalSensors)
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        try
        {
            var session = new PollingSession
            {
                MonitoringPostId = postId,
                TotalSensorsCount = totalSensors,
                Status = "IN_PROGRESS",
                StartedAt = DateTime.UtcNow
            };
            
            context.PollingSessions.Add(session);
            
            var post = await context.MonitoringPosts.FindAsync(postId);
            if (post != null)
            {
                post.LastPolledAt = session.StartedAt;
            }
            
            await context.SaveChangesAsync();
            return session.Id;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting polling session for post {postId}", postId);
            return Guid.Empty;
        }
    }

    /// <summary>
    /// Завершение сессии опроса.
    /// </summary>
    public async Task FinishPollingSessionAsync(Guid sessionId, int successCount, string? errorDetails)
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        try
        {
            var session = await context.PollingSessions.FindAsync(sessionId);
            if (session != null)
            {
                session.CompletedAt = DateTime.UtcNow;
                session.SuccessfulSensorsCount = successCount;
                session.FailedSensorsDetails = errorDetails;
                
                if (successCount == 0 && session.TotalSensorsCount > 0)
                    session.Status = "FAILED";
                else if (successCount < session.TotalSensorsCount)
                    session.Status = "PARTIALLY_COMPLETED";
                else
                    session.Status = "COMPLETED";
                
                await context.SaveChangesAsync();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error finishing polling session {sessionId}", sessionId);
        }
    }

    /// <summary>
    /// Получение списка активных датчиков.
    /// </summary>
    public async Task<List<Sensor>> GetActiveSensorsAsync()
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        try
        {
            return await context.Sensors
                .Include(s => s.SensorType)
                .Include(s => s.MonitoringPost)
                .Where(s => s.IsActive && s.MonitoringPost != null && s.MonitoringPost.IsActive)
                .AsNoTracking()
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting active sensors");
            throw;
        }
    }

    /// <summary>
    /// Получение данных одного датчика по его идентификатору.
    /// </summary>
    public Sensor? GetSensorById(int sensorId)
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        try
        {
            return context.Sensors
                .AsNoTracking()
                .FirstOrDefault(s => s.Id == sensorId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting sensor by id {sensorId}", sensorId);
            return null;
        }
    }

    /// <summary>
    /// Получение списка датчиков по набору идентификаторов.
    /// </summary>
    public List<Sensor> GetSensorsByIds(List<int> sensorIds)
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        try
        {
            return context.Sensors
                .Include(s => s.SensorType)
                .Where(s => sensorIds.Contains(s.Id))
                .AsNoTracking()
                .ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting sensors by ids");
            return new List<Sensor>();
        }
    }

    /// <summary>
    /// Сохранение метаданных результата опроса.
    /// </summary>
    public async Task SaveSensorResultAsync(SensorResults result)
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        try
        {
            context.SensorResults.Add(result);
            await context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving sensor result for sensor {sensorId}", result.SensorId);
        }
    }

    /// <summary>
    /// Обновление метки времени последней активности датчика.
    /// </summary>
    public async Task UpdateSensorLastActivityAsync(int sensorId)
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        try
        {
            var sensor = await context.Sensors.FindAsync(sensorId);
            if (sensor != null)
            {
                sensor.LastActivityUTC = DateTime.UtcNow;
                await context.SaveChangesAsync();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating last activity for sensor {sensorId}", sensorId);
        }
    }

    /// <summary>
    /// Получение списка активных PUID, которые пора опрашивать.
    /// </summary>
    public async Task<List<Puid>> GetPuidsToPollAsync()
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        try
        {
            var now = DateTime.UtcNow;
            return await context.Puids
                .Where(p => p.IsActive && 
                           (p.LastActivityUTC == null || 
                            p.LastActivityUTC.Value.AddSeconds(p.IntervalSeconds) <= now))
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting PUIDs to poll");
            return new List<Puid>();
        }
    }

    /// <summary>
    /// Сохранение результата опроса PUID.
    /// </summary>
    public async Task SavePuidResultAsync(PuidResults result)
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        try
        {
            context.PuidResults.Add(result);
            await context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving PUID result for PUID {puidId}", result.PuidId);
        }
    }

    /// <summary>
    /// Сохранение данных PUID и обновление времени последней активности.
    /// </summary>
    public async Task SavePuidDataAsync(int puidId, List<PuidData> dataList)
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        try
        {
            var puid = await context.Puids.FindAsync(puidId);
            if (puid != null)
            {
                puid.LastActivityUTC = DateTime.UtcNow;
                if (dataList.Any())
                {
                    await context.PuidData.AddRangeAsync(dataList);
                }
                await context.SaveChangesAsync();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving PUID data for PUID {puidId}", puidId);
        }
    }

    /// <summary>
    /// Сохранение информации об ошибке опроса в БД.
    /// </summary>
    public async Task SaveSensorErrorAsync(SensorError error)
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        try
        {
            context.SensorError.Add(error);
            await context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving sensor error for sensor {sensorId}", error.SensorId);
        }
    }

    /// <summary>
    /// Загрузка активных параметров конфигурации воркера из БД.
    /// </summary>
    public async Task<List<WorkerConfiguration>> GetConfigurationAsync()
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        try
        {
            return await context.WorkerConfigurations
                .Where(c => c.IsActive)
                .AsNoTracking()
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting configuration");
            return new List<WorkerConfiguration>();
        }
    }

    /// <summary>
    /// Сохранение данных дорожной станции (DSPD).
    /// </summary>
    public async Task SaveDspdDataAsync(DSPDData data, Guid? pollingSessionId = null, int? monitoringPostId = null)
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        try
        {
            data.PollingSessionId = pollingSessionId;
            data.MonitoringPostId = monitoringPostId;
            context.DSPDData.Add(data);
            await context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving DSPD data for sensor {sensorId}", data.SensorId);
        }
    }

    /// <summary>
    /// Сохранение данных метеостанции (IWS).
    /// </summary>
    public async Task SaveIwsDataAsync(IWSData data, Guid? pollingSessionId = null, int? monitoringPostId = null)
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        try
        {
            data.PollingSessionId = pollingSessionId;
            data.MonitoringPostId = monitoringPostId;
            context.IWSData.Add(data);
            await context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving IWS data for sensor {sensorId}", data.SensorId);
        }
    }

    /// <summary>
    /// Сохранение данных станции мониторинга воздуха (MUEKS).
    /// </summary>
    public async Task SaveMueksDataAsync(MUEKSData data, Guid? pollingSessionId = null, int? monitoringPostId = null)
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        try
        {
            data.PollingSessionId = pollingSessionId;
            data.MonitoringPostId = monitoringPostId;
            context.MUEKSData.Add(data);
            await context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving MUEKS data for sensor {sensorId}", data.SensorId);
        }
    }

    /// <summary>
    /// Сохранение данных датчика видимости (DOV).
    /// </summary>
    public async Task SaveDovDataAsync(DOVData data, Guid? pollingSessionId = null, int? monitoringPostId = null)
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        try
        {
            data.PollingSessionId = pollingSessionId;
            data.MonitoringPostId = monitoringPostId;
            context.DOVData.Add(data);
            await context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving DOV data for sensor {sensorId}", data.SensorId);
        }
    }

    /// <summary>
    /// Сохранение данных датчика пыли (DUST).
    /// </summary>
    public async Task SaveDustDataAsync(DUSTData data, Guid? pollingSessionId = null, int? monitoringPostId = null)
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        try
        {
            data.PollingSessionId = pollingSessionId;
            data.MonitoringPostId = monitoringPostId;
            context.DustData.Add(data);
            await context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving DUST data for sensor {sensorId}", data.SensorId);
        }
    }
     /// <summary>
    /// Сохранение данных датчика пыли (DUST).
    /// </summary>
    public async Task SaveDustDataAsync(DUSTData data)
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        try
        {
            context.DustData.Add(data);
            await context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving DUST data for sensor {sensorId}", data.SensorId);
        }
    }

    /// <summary>
    /// Обновление серийного номера датчика в БД.
    /// </summary>
    public async Task UpdateSensorSerialNumberAsync(int sensorId, string serialNumber)
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        try
        {
            var sensor = await context.Sensors.FindAsync(sensorId);
            if (sensor != null)
            {
                serialNumber = ExtractSerialNumber(serialNumber);
                if (sensor.SerialNumber != serialNumber)
                {
                    sensor.SerialNumber = serialNumber;
                    await context.SaveChangesAsync();
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating serial number for sensor {sensorId}", sensorId);
        }
    }

    private string ExtractSerialNumber(string input)
    {
        if (string.IsNullOrEmpty(input)) return input;
        string[] parts = input.Split('_');
        return parts.Length > 1 ? parts[1] : input;
    }

    public async Task UpdateDspdSensorCoordinatesAsync(int sensorId, decimal? latitude, decimal? longitude)
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        try
        {
            var sensor = await context.Sensors.FindAsync(sensorId);
            if (sensor != null && latitude.HasValue && longitude.HasValue)
            {
                sensor.Latitude = (double?)latitude;
                sensor.Longitude = (double?)longitude;
                await context.SaveChangesAsync();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating coordinates for sensor {sensorId}", sensorId);
        }
    }

    public async Task UpdateIwsSensorCoordinatesAsync(int sensorId, decimal? latitude, decimal? longitude)
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        try
        {
            var sensor = await context.Sensors.FindAsync(sensorId);
            if (sensor != null && latitude.HasValue && longitude.HasValue)
            {
                sensor.Latitude = (double?)latitude;
                sensor.Longitude = (double?)longitude;
                await context.SaveChangesAsync();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating coordinates for sensor {sensorId}", sensorId);
        }
    }
}
