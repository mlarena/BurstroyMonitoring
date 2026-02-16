using System.Globalization;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using BurstroyMonitoring.Data.Models;
using BurstroyMonitoring.Data.Models.JsonModels;

namespace BurstroyMonitoring.Worker.Services;

/// <summary>
/// Сервис для обработки данных датчиков
/// </summary>
public class DataProcessingService
{
    private readonly DatabaseService _dbService;
    private readonly LoggerService _loggerService;
    private readonly ILogger<DataProcessingService> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public DataProcessingService(
        DatabaseService dbService,
        LoggerService loggerService,
        ILogger<DataProcessingService> logger)
    {
        _dbService = dbService;
        _loggerService = loggerService;
        _logger = logger;
        
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowReadingFromString
        };
    }

    /// <summary>
    /// Обработка данных датчика
    /// </summary>
    public async Task ProcessSensorDataAsync(Sensor sensor, JsonDocument jsonDocument, CancellationToken cancellationToken)
    {
        try
        {
            var jsonString = jsonDocument.RootElement.GetRawText();

          
            string? serialNumber = null;
            if (jsonDocument.RootElement.TryGetProperty("Serial", out var serialElement))
            {
                serialNumber = serialElement.GetString();
            }
            
            // Если нашли серийный номер - обновляем в базе
            if (!string.IsNullOrEmpty(serialNumber))
            {
                await _dbService.UpdateSensorSerialNumberAsync(sensor.Id, serialNumber);
            }
            
            switch (sensor.SensorType?.SensorTypeName)
            {
                case "DSPD":
                    await ProcessDspdDataAsync(sensor, jsonString, cancellationToken);
                    break;
                case "IWS":
                    await ProcessIwsDataAsync(sensor, jsonString, cancellationToken);
                    break;
                case "DOV":
                    await ProcessDovDataAsync(sensor, jsonString, cancellationToken);
                    break;
                case "MUEKS":
                    await ProcessMueksDataAsync(sensor, jsonString, cancellationToken);
                    break;
                case "DUST":
                    await ProcessDustDataAsync(sensor, jsonString, cancellationToken);
                    break;
                default:
                    _logger.LogWarning("Unknown sensor type: {sensorType} for sensor {sensorId}", 
                        sensor.SensorType?.SensorTypeName, sensor.Id);
                    break;
            }
        }
        catch (JsonException jsonEx)
        {
            _logger.LogError(jsonEx, "JSON deserialization error for sensor {sensorId}", sensor.Id);
            await _loggerService.LogDatabaseErrorAsync(sensor, "JSON_PARSE_ERROR", jsonEx.Message, jsonEx);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing data for sensor {sensorId}", sensor.Id);
            await _loggerService.LogDatabaseErrorAsync(sensor, "DATA_PROCESSING_ERROR", ex.Message, ex);
        }
    }



    /// <summary>
    /// Обработка данных DSPD
    /// </summary>
    private async Task ProcessDspdDataAsync(Sensor sensor, string jsonString, CancellationToken cancellationToken)
    {
        try
        {
            var jsonModel = JsonSerializer.Deserialize<DspdJsonModel>(jsonString, _jsonOptions);
            
            if (jsonModel?.Packet == null)
            {
                throw new JsonException("Invalid DSPD JSON structure");
            }

            var packet = jsonModel.Packet;
            var dataTimestamp = ParseDateTimeToUtc(packet.DataTime, "dd-MM-yyyy,HH:mm:ss");

            var dspdData = new DSPDData
            {
                SensorId = sensor.Id,
                ReceivedAt = DateTime.UtcNow,
                DataTimestamp = dataTimestamp,
                Grip = packet.Grip,
                Shake = packet.Shake,
                UPower = packet.UPower,
                TemperatureCase = packet.TemperatureCase,
                TemperatureRoad = packet.TemperatureRoad,
                HeightH2O = packet.HeightH2O,
                HeightIce = packet.HeightIce,
                HeightSnow = packet.HeightSnow,
                PercentICE = packet.PercentICE,
                PercentPGM = packet.PercentPGM,
                RoadStatus = packet.RoadStatus,
                AngleToRoad = packet.AngleToRoad,
                TemperatureFreezePGM = packet.TemperatureFreezePGM,
                NeedCalibration = packet.NeedCalibration,
                DistanceToSurface = packet.DistanceToSurface
            };

           // Обработка GPS координат из пакета
            if (packet.GPSLatitude != null && packet.GPSLongitude != null)
            {
                var (latitude, longitude, isValid) = ParseGpsCoordinates(packet.GPSLatitude, packet.GPSLongitude);
                
                dspdData.GPSLatitude = latitude;
                dspdData.GPSLongitude = longitude;
                dspdData.IsGpsValid = isValid;
                
                // ОБНОВЛЯЕМ КООРДИНАТЫ ДАТЧИКА в таблице Sensor
                if (isValid && latitude.HasValue && longitude.HasValue)
                {
                    await _dbService.UpdateDspdSensorCoordinatesAsync(sensor.Id, latitude, longitude);
       
                }
            }

            // Вызов реального метода сохранения из DatabaseService
            await _dbService.SaveDspdDataAsync(dspdData);
            
            // Информативное логирование
            string sensorInfo = $"sensor '{sensor.SerialNumber}'";
            string postInfo = sensor.MonitoringPost != null 
                ? $" on post '{sensor.MonitoringPost.Name}'" 
                : "";
            
            _logger.LogInformation("DSPD data saved for {sensorInfo}{postInfo} at {timestamp:yyyy-MM-dd HH:mm:ss}", 
                sensorInfo, postInfo, dataTimestamp);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing DSPD data for sensor '{SerialNumber}'", sensor.SerialNumber);
            await _loggerService.LogDatabaseErrorAsync(sensor, "DSPD_PROCESSING_ERROR", ex.Message, ex);
        }
    }

    // Обновите аналогично другие методы ProcessIwsDataAsync, ProcessMueksDataAsync, ProcessDovDataAsync:

    /// <summary>
    /// Обработка данных IWS
    /// </summary>
    private async Task ProcessIwsDataAsync(Sensor sensor, string jsonString, CancellationToken cancellationToken)
    {
        try
        {
            var jsonModel = JsonSerializer.Deserialize<IwsJsonModel>(jsonString, _jsonOptions);
            
            if (jsonModel?.Packet == null)
            {
                throw new JsonException("Invalid IWS JSON structure");
            }

            var packet = jsonModel.Packet;
            var dataTimestamp = ParseDateTimeToUtc(packet.DataTime, "dd-MM-yyyy,HH:mm:ss.fff");

            var iwsData = new IWSData
            {
                SensorId = sensor.Id,
                ReceivedAt = DateTime.UtcNow,
                DataTimestamp = dataTimestamp,
                EnvTemperature = packet.EnvTemperature,
                Humidity = packet.Humidity,
                DewPoint = packet.DewPoint,
                PressureHPa = packet.PressureHPa,
                PressureQNHHPa = packet.PressureQNHHPa,
                PressureMmHg = packet.PressureMmHg,
                WindSpeed = packet.WindSpeed,
                WindDirection = packet.WindDirection,
                WindVSound = packet.WindVSound,
                PrecipitationType = packet.PrecipitationType,
                PrecipitationIntensity = packet.PrecipitationIntensity,
                PrecipitationQuantity = packet.PrecipitationQuantity,
                PrecipitationElapsed = packet.PrecipitationElapsed,
                PrecipitationPeriod = packet.PrecipitationPeriod,
                CO2Level = packet.CO2Level,
                SupplyVoltage = packet.SupplyVoltage,
                Latitude = packet.Latitude,
                Longitude = packet.Longitude,
                Altitude = packet.Altitude,
                KSP = packet.KSP,
                GPSSpeed = packet.GPSSpeed,
                AccelerationStDev = packet.AccelerationStDev,
                Roll = packet.Roll,
                Pitch = packet.Pitch,
                WeAreFine = packet.WeAreFine
            };

            // ОБНОВЛЯЕМ КООРДИНАТЫ ДАТЧИКА в таблице Sensor
            if (packet.Latitude.HasValue && packet.Longitude.HasValue)
            {
                await _dbService.UpdateIwsSensorCoordinatesAsync(sensor.Id, packet.Latitude, packet.Longitude, packet.Altitude);
               
            }

            await _dbService.SaveIwsDataAsync(iwsData);
            
            string sensorInfo = $"sensor '{sensor.SerialNumber}'";
            string postInfo = sensor.MonitoringPost != null 
                ? $" on post '{sensor.MonitoringPost.Name}'" 
                : "";
            
            _logger.LogInformation("IWS data saved for {sensorInfo}{postInfo} at {timestamp:yyyy-MM-dd HH:mm:ss.fff}", 
                sensorInfo, postInfo, dataTimestamp);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing IWS data for sensor '{SerialNumber}'", sensor.SerialNumber);
            await _loggerService.LogDatabaseErrorAsync(sensor, "IWS_PROCESSING_ERROR", ex.Message, ex);
        }
    }

    /// <summary>
    /// Обработка данных MUEKS
    /// </summary>
    private async Task ProcessMueksDataAsync(Sensor sensor, string jsonString, CancellationToken cancellationToken)
    {
        try
        {
            var jsonModel = JsonSerializer.Deserialize<MueksJsonModel>(jsonString, _jsonOptions);
            
            if (jsonModel?.Packet == null)
            {
                throw new JsonException("Invalid MUEKS JSON structure");
            }

            var packet = jsonModel.Packet;
            var dataTimestamp = ParseDateTimeToUtc(packet.DataTime, "dd-MM-yyyy,HH:mm:ss");

            var mueksData = new MUEKSData
            {
                SensorId = sensor.Id,
                ReceivedAt = DateTime.UtcNow,
                DataTimestamp = dataTimestamp,
                TemperatureBox = packet.TemperatureBox,
                UPowerIn12B = packet.UPowerIn12B,
                UOut12B = packet.UOut12B,
                IOut12B = packet.IOut12B,
                IOut48B = packet.IOut48B,
                UAkb = packet.UAkb,
                IAkb = packet.IAkb,
                Sens220B = packet.Sens220B,
                WhAkb = packet.WhAkb,
                VisibleRange = packet.VisibleRange,
                DoorStatus = packet.DoorStatus,
                TdsH = packet.TdsH,
                TdsTds = packet.TdsTds,
                TkosaT1 = packet.TkosaT1,
                TkosaT3 = packet.TkosaT3
            };

            await _dbService.SaveMueksDataAsync(mueksData);
            
            string sensorInfo = $"sensor '{sensor.SerialNumber}'";
            string postInfo = sensor.MonitoringPost != null 
                ? $" on post '{sensor.MonitoringPost.Name}'" 
                : "";
            
            _logger.LogInformation("MUEKS data saved for {sensorInfo}{postInfo} at {timestamp:yyyy-MM-dd HH:mm:ss}", 
                sensorInfo, postInfo, dataTimestamp);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing MUEKS data for sensor '{SerialNumber}'", sensor.SerialNumber);
            await _loggerService.LogDatabaseErrorAsync(sensor, "MUEKS_PROCESSING_ERROR", ex.Message, ex);
        }
    }

    /// <summary>
    /// Обработка данных DOV
    /// </summary>
    private async Task ProcessDovDataAsync(Sensor sensor, string jsonString, CancellationToken cancellationToken)
    {
        try
        {
            var jsonModel = JsonSerializer.Deserialize<DovJsonModel>(jsonString, _jsonOptions);
            
            if (jsonModel?.Packet == null)
            {
                throw new JsonException("Invalid DOV JSON structure");
            }

            var packet = jsonModel.Packet;
            
            var dovData = new DOVData
            {
                SensorId = sensor.Id,
                ReceivedAt = DateTime.UtcNow,
                DataTimestamp = DateTime.UtcNow,
                VisibleRange = packet.VisibleRange,
                BrightFlag = packet.BrightFlag
            };

            await _dbService.SaveDovDataAsync(dovData);
            
            string sensorInfo = $"sensor '{sensor.SerialNumber}'";
            string postInfo = sensor.MonitoringPost != null 
                ? $" on post '{sensor.MonitoringPost.Name}'" 
                : "";
            
            _logger.LogInformation("DOV data saved for {sensorInfo}{postInfo}", 
                sensorInfo, postInfo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing DOV data for sensor '{SerialNumber}'", sensor.SerialNumber);
            await _loggerService.LogDatabaseErrorAsync(sensor, "DOV_PROCESSING_ERROR", ex.Message, ex);
        }
    }

    /// <summary>
    /// Обработка данных DUST
    /// </summary>
    private async Task ProcessDustDataAsync(Sensor sensor, string jsonString, CancellationToken cancellationToken)
    {
        try
        {
            var jsonModel = JsonSerializer.Deserialize<DustJsonModel>(jsonString, _jsonOptions);
            
            if (jsonModel?.Packet == null)
            {
                throw new JsonException("Invalid DUST JSON structure");
            }

            var packet = jsonModel.Packet;
            
            var dustData = new DUSTData
            {
                SensorId = sensor.Id,
                ReceivedAt = DateTime.UtcNow,
                DataTimestamp = DateTime.UtcNow, 
                PM10Act = packet.PM10_act,
                PM25Act = packet.PM2_5_act,
                PM1Act = packet.PM1_0_act,
                PM10AWG = packet.PM10_awg,
                PM25AWG = packet.PM2_5_awg,
                PM1AWG = packet.PM1_0_awg,
                FlowProbe = packet.Flow_probe,
                TemperatureProbe = packet.Temperature_probe,
                HumidityProbe = packet.Humidity_probe,
                LaserStatus = packet.Laser_status,
                SupplyVoltage = packet.Supply_voltage
            };

            await _dbService.SaveDustDataAsync(dustData);
            
            string sensorInfo = $"sensor '{sensor.SerialNumber}'";
            string postInfo = sensor.MonitoringPost != null 
                ? $" on post '{sensor.MonitoringPost.Name}'" 
                : "";
            
            _logger.LogInformation("DUST data saved for {sensorInfo}{postInfo}", 
                sensorInfo, postInfo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing DUST data for sensor '{SerialNumber}'", sensor.SerialNumber);
            await _loggerService.LogDatabaseErrorAsync(sensor, "DUST_PROCESSING_ERROR", ex.Message, ex);
        }
    }


    /// <summary>
    /// Парсинг даты-времени из строки с преобразованием в UTC
    /// </summary>
    private DateTime ParseDateTimeToUtc(string? dateTimeString, string format)
    {
        if (string.IsNullOrEmpty(dateTimeString))
            return DateTime.UtcNow;

        try
        {
            DateTime parsedDateTime;
            
            if (DateTime.TryParseExact(dateTimeString, format, 
                CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedDateTime))
            {
                // Если дата имеет Kind = Unspecified, предполагаем, что это локальное время
                if (parsedDateTime.Kind == DateTimeKind.Unspecified)
                {
                    // Преобразуем в UTC через локальное время
                    return DateTime.SpecifyKind(parsedDateTime, DateTimeKind.Utc);
                }
                
                // Если уже UTC, возвращаем как есть
                if (parsedDateTime.Kind == DateTimeKind.Utc)
                    return parsedDateTime;
                
                // Если Local, конвертируем в UTC
                return parsedDateTime.ToUniversalTime();
            }
            
            // Попытка парсинга как обычной даты
            if (DateTime.TryParse(dateTimeString, CultureInfo.InvariantCulture, 
                DateTimeStyles.None, out parsedDateTime))
            {
                if (parsedDateTime.Kind == DateTimeKind.Unspecified)
                {
                    return DateTime.SpecifyKind(parsedDateTime, DateTimeKind.Utc);
                }
                
                if (parsedDateTime.Kind == DateTimeKind.Utc)
                    return parsedDateTime;
                
                return parsedDateTime.ToUniversalTime();
            }
        }
        catch (Exception ex)
        {
            // Игнорируем ошибки парсинга даты
            _logger.LogError("--ParseDateTimeToUtc--" + ex.Message.ToString());
        }

        return DateTime.UtcNow;
    }
 
    /// <summary>
    /// Парсинг GPS координат с поддержкой значений "err"
    /// </summary>
    private (decimal? latitude, decimal? longitude, bool isValid) ParseGpsCoordinates(object? latObj, object? lonObj)
    {
        try
        {
            if (latObj == null || lonObj == null)
                return (null, null, false);

            var latStr = latObj.ToString();
            var lonStr = lonObj.ToString();

            // Проверка на значение "err"
            if (string.Equals(latStr, "err", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(lonStr, "err", StringComparison.OrdinalIgnoreCase))
                return (null, null, false);

            // Попытка парсинга decimal
            if (decimal.TryParse(latStr, NumberStyles.Any, CultureInfo.InvariantCulture, out var lat) &&
                decimal.TryParse(lonStr, NumberStyles.Any, CultureInfo.InvariantCulture, out var lon))
            {
                // Проверка валидности координат
                bool isValid = lat >= -90 && lat <= 90 && lon >= -180 && lon <= 180;
                return (lat, lon, isValid);
            }
        }
        catch (Exception ex)
        {
            // Игнорируем ошибки парсинга даты
            _logger.LogError("--ParseGpsCoordinates--" + ex.Message.ToString());
        }

        return (null, null, false);
    }
}