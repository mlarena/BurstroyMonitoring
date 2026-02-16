using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using BurstroyMonitoring.Data.Models;
using BurstroyMonitoring.Data;


namespace BurstroyMonitoring.Worker.Services;

/// <summary>
/// Сервис для работы с базой данных
/// </summary>
public class DatabaseService
{
    private readonly ILogger<DatabaseService> _logger;
    private readonly IServiceProvider _serviceProvider;

    public DatabaseService(
        ILogger<DatabaseService> logger,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    /// <summary>
    /// Создание scope и получение контекста
    /// </summary>
    private ApplicationDbContext CreateDbContext()
    {
        var scope = _serviceProvider.CreateScope();
        return scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    }

    /// <summary>
    /// Получение активных датчиков из базы данных
    /// </summary>
    public async Task<List<Sensor>> GetActiveSensorsAsync()
    {
        using var context = CreateDbContext();
        
        try
        {
            return await context.Sensors
                .Include(s => s.SensorType)
                .Include(s => s.MonitoringPost) // Добавлено включение MonitoringPost
                .Where(s => s.IsActive)
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
    /// Получение датчика по ID
    /// </summary>
    public Sensor? GetSensorById(int sensorId)
    {
        using var context = CreateDbContext();
        
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
    /// Получение датчиков по списку ID
    /// </summary>
    public List<Sensor> GetSensorsByIds(List<int> sensorIds)
    {
        using var context = CreateDbContext();
        
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
    /// Сохранение результата опроса датчика
    /// </summary>
    public async Task SaveSensorResultAsync(SensorResults result)
    {
        using var context = CreateDbContext();
        
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
    /// Обновление времени последней активности датчика
    /// </summary>
    public async Task UpdateSensorLastActivityAsync(int sensorId)
    {
        using var context = CreateDbContext();
        
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
    /// Сохранение ошибки в базу данных
    /// </summary>
    public async Task SaveSensorErrorAsync(SensorError error)
    {
        using var context = CreateDbContext();
        
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
    /// Получение конфигурации из базы данных
    /// </summary>
    public async Task<List<WorkerConfiguration>> GetConfigurationAsync()
    {
        using var context = CreateDbContext();
        
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
    /// Сохранение данных DSPD
    /// </summary>
    public async Task SaveDspdDataAsync(DSPDData data)
    {
        using var context = CreateDbContext();
        
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
    /// Сохранение данных IWS
    /// </summary>
    public async Task SaveIwsDataAsync(IWSData data)
    {
        using var context = CreateDbContext();
        
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
    /// Сохранение данных MUEKS
    /// </summary>
    public async Task SaveMueksDataAsync(MUEKSData data)
    {
        using var context = CreateDbContext();
        
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
    /// Сохранение данных DOV
    /// </summary>
    public async Task SaveDovDataAsync(DOVData data)
    {
        using var context = CreateDbContext();
        
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

    public async Task SaveDustDataAsync(DUSTData data)
    {
        using var context = CreateDbContext();
        
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
    /// Обновление серийного номера датчика при первом успешном опросе
    /// </summary>
    public async Task UpdateSensorSerialNumberAsync(int sensorId, string serialNumber)
    {
        using var context = CreateDbContext();
        
        try
        {
            var sensor = await context.Sensors.FindAsync(sensorId);
            if (sensor != null)
            {
                // Обновляем всегда, если серийный номер отличается
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
    public string ExtractSerialNumber(string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;
        
        string[] parts = input.Split('_');
        return parts.Length > 1 ? parts[1] : input;
    }

    /// <summary>
    /// Обновление координат датчика DSPD
    /// </summary>
    public async Task UpdateDspdSensorCoordinatesAsync(int sensorId, decimal? latitude, decimal? longitude)
    {
        using var context = CreateDbContext();
        
        try
        {
            var sensor = await context.Sensors.FindAsync(sensorId);
            if (sensor != null && latitude.HasValue && longitude.HasValue)
            {
                // Проверяем валидность координат
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
    /// Обновление координат датчика IWS
    /// </summary>
    public async Task UpdateIwsSensorCoordinatesAsync(int sensorId, decimal? latitude, decimal? longitude, decimal? altitude)
    {
        using var context = CreateDbContext();
        
        try
        {
            var sensor = await context.Sensors.FindAsync(sensorId);
            if (sensor != null && latitude.HasValue && longitude.HasValue)
            {
                // Проверяем валидность координат
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