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
    /// Получение списка активных датчиков.
    /// Датчик считается активным, если у него IsActive = true и его пост мониторинга также активен.
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
    /// Сохранение метаданных результата опроса (статус-код, время ответа).
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
    public async Task SaveDspdDataAsync(DSPDData data)
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        try
        {
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
    public async Task SaveIwsDataAsync(IWSData data)
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        try
        {
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
    public async Task SaveMueksDataAsync(MUEKSData data)
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        try
        {
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
    public async Task SaveDovDataAsync(DOVData data)
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        try
        {
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
                    
                    _logger.LogInformation("Updated serial number for sensor {sensorId}: {serialNumber}", 
                        sensorId, serialNumber);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating serial number for sensor {sensorId}", sensorId);
        }
    }

    /// <summary>
    /// Вспомогательный метод для извлечения чистого серийного номера.
    /// </summary>
    public string ExtractSerialNumber(string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;
        
        string[] parts = input.Split('_');
        return parts.Length > 1 ? parts[1] : input;
    }

    /// <summary>
    /// Обновление географических координат для датчика DSPD.
    /// </summary>
    public async Task UpdateDspdSensorCoordinatesAsync(int sensorId, decimal? latitude, decimal? longitude)
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        try
        {
            var sensor = await context.Sensors.FindAsync(sensorId);
            if (sensor != null && latitude.HasValue && longitude.HasValue)
            {
                if (latitude >= -90 && latitude <= 90 && longitude >= -180 && longitude <= 180)
                {
                    sensor.Latitude = (double?)latitude;
                    sensor.Longitude = (double?)longitude;
                    await context.SaveChangesAsync();
                    
                    _logger.LogInformation("Updated coordinates for DSPD sensor {sensorId}: Lat={latitude}, Lon={longitude}", 
                        sensorId, latitude, longitude);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating coordinates for DSPD sensor {sensorId}", sensorId);
        }
    }

    /// <summary>
    /// Обновление географических координат для датчика IWS.
    /// </summary>
    public async Task UpdateIwsSensorCoordinatesAsync(int sensorId, decimal? latitude, decimal? longitude, decimal? altitude)
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        try
        {
            var sensor = await context.Sensors.FindAsync(sensorId);
            if (sensor != null && latitude.HasValue && longitude.HasValue)
            {
                if (latitude >= -90 && latitude <= 90 && longitude >= -180 && longitude <= 180)
                {
                    sensor.Latitude = (double?)latitude;
                    sensor.Longitude = (double?)longitude;
                    await context.SaveChangesAsync();
                    
                    _logger.LogInformation("Updated coordinates for IWS sensor {sensorId}: Lat={latitude}, Lon={longitude}, Alt={altitude}", 
                        sensorId, latitude, longitude, altitude);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating coordinates for IWS sensor {sensorId}", sensorId);
        }
    }
}
