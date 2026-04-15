using System.Globalization;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using BurstroyMonitoring.Data.Models;
using BurstroyMonitoring.Data.Models.JsonModels;
using System.Net.Http.Json;

namespace BurstroyMonitoring.Worker.Services;

/// <summary>
/// Сервис для обработки данных, полученных от датчиков.
/// Отвечает за десериализацию JSON и сохранение в соответствующие таблицы БД.
/// </summary>
public class DataProcessingService
{
    private readonly DatabaseService _dbService;
    private readonly LoggerService _loggerService;
    private readonly SensorPollingService _pollingService;
    private readonly ILogger<DataProcessingService> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public DataProcessingService(
        DatabaseService dbService,
        LoggerService loggerService,
        SensorPollingService pollingService,
        ILogger<DataProcessingService> logger)
    {
        _dbService = dbService;
        _loggerService = loggerService;
        _pollingService = pollingService;
        _logger = logger;
        
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowReadingFromString
        };
    }

    /// <summary>
    /// Выполняет полный цикл опроса одного PUID-устройства: запрос, парсинг и сохранение.
    /// </summary>
    public async Task ProcessPuidPollingAsync(Puid puid, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("[INFO] Processing PUID: {EndPointsName} (ID: {Id})", puid.EndPointsName, puid.Id);

            var tempSensor = new Sensor 
            { 
                Url = puid.Url, 
                SerialNumber = puid.SerialNumber,
                Id = -puid.Id 
            };

            var (responseBody, statusCode, responseTimeMs, isSuccess, exception) = 
                await _pollingService.PollSensorAsync(tempSensor, cancellationToken);

            if (!isSuccess || string.IsNullOrEmpty(responseBody))
            {
                _logger.LogWarning("[ERROR] API Request failed for {EndPointsName}: Status {StatusCode}", puid.EndPointsName, statusCode);
                return;
            }

            SmartRoadPuidResult? response;
            try
            {
                response = JsonSerializer.Deserialize<SmartRoadPuidResult>(responseBody, _jsonOptions);
            }
            catch (JsonException ex)
            {
                _logger.LogError("[ERROR] JSON Deserialization failed for {EndPointsName}: {Message}", puid.EndPointsName, ex.Message);
                return;
            }

            if (response?.message_data == null || string.IsNullOrEmpty(response.message_id))
            {
                _logger.LogWarning("[WARNING] No valid data received for {EndPointsName}.", puid.EndPointsName);
                return;
            }

            if (!Guid.TryParse(response.message_id, out Guid messageId))
            {
                _logger.LogError("[ERROR] Invalid MessageId format for {EndPointsName}: {MessageId}", puid.EndPointsName, response.message_id);
                return;
            }

            var dataToSave = new List<PuidData>();
            int savedInThisRequest = 0;

            foreach (var message in response.message_data)
            {
                if (!Guid.TryParse(message.sensor_id, out Guid sensorId))
                {
                    _logger.LogWarning("[WARNING] Skipping sensor with invalid ID in {EndPointsName}: {SensorId}", puid.EndPointsName, message.sensor_id);
                    continue;
                }

                foreach (var interval in message.data)
                {
                    foreach (var lane in interval.lanes)
                    {
                        var entity = new PuidData
                        {
                            PuidId = puid.Id,
                            MessageId = messageId,
                            SensorId = sensorId,
                            SensorName = message.name,
                            Direction = message.direction,
                            Lane = lane.lane,
                            Volume = lane.volume,
                            Class0 = lane.class_0,
                            Class1 = lane.class_1,
                            Class2 = lane.class_2,
                            Class3 = lane.class_3,
                            Class4 = lane.class_4,
                            Class5 = lane.class_5,
                            GapAvg = lane.gap_avg,
                            GapSum = lane.gap_sum,
                            SpeedAvg = lane.speed_avg,
                            HeadwayAvg = lane.headway_avg,
                            HeadwaySum = lane.headway_sum,
                            Speed85Avg = lane.speed85_avg,
                            OccupancyPer = lane.occupancy_per,
                            OccupancyPrc = lane.occupancy_prc,
                            OccupancySum = lane.occupancy_sum,
                            RangeStart = DateTime.SpecifyKind(interval.range_start, DateTimeKind.Utc),
                            RangeEnd = DateTime.SpecifyKind(interval.range_end, DateTimeKind.Utc),
                            RangeValue = interval.range_value,
                            CreatedAt = DateTime.UtcNow
                        };

                        dataToSave.Add(entity);
                        savedInThisRequest++;
                    }
                }
            }

            if (savedInThisRequest > 0)
            {
                await _dbService.SavePuidDataAsync(puid.Id, dataToSave);
                _logger.LogInformation("[SUCCESS] {EndPointsName}: Saved {Count} records (MessageId: {MessageId})", 
                    puid.EndPointsName, savedInThisRequest, messageId);
            }
            else
            {
                await _dbService.SavePuidDataAsync(puid.Id, new List<PuidData>());
            }
        }
        catch (Exception ex)
        {
            _logger.LogError("[ERROR] Failed to process {EndPointsName}: {Message}", puid.EndPointsName, ex.Message);
        }
    }

    /// <summary>
    /// Опрос всех датчиков на конкретном посту мониторинга.
    /// </summary>
    public async Task ProcessPostPollingAsync(MonitoringPost post, CancellationToken cancellationToken)
    {
        var sensors = post.Sensors.Where(s => s.IsActive).ToList();
        if (sensors.Count == 0) return;

        _logger.LogInformation("Starting polling session for post '{PostName}' (ID: {PostId}) with {Count} sensors", 
            post.Name, post.Id, sensors.Count);

        var sessionId = await _dbService.StartPollingSessionAsync(post.Id, sensors.Count);
        if (sessionId == Guid.Empty) return;

        int successCount = 0;
        var errors = new List<string>();

        try
        {
            var pollingTasks = sensors.Select(async sensor =>
            {
                try
                {
                    var (responseBody, statusCode, responseTimeMs, isSuccess, exception) = await _pollingService.PollSensorAsync(sensor, cancellationToken);

                    if (isSuccess && !string.IsNullOrEmpty(responseBody))
                    {
                        var result = new SensorResults
                        {
                            SensorId = sensor.Id,
                            CheckedAt = DateTime.UtcNow,
                            StatusCode = statusCode,
                            ResponseTimeMs = responseTimeMs,
                            IsSuccess = isSuccess,
                            ResponseBody = JsonDocument.Parse(responseBody),
                            PollingSessionId = sessionId
                        };
                        await _dbService.SaveSensorResultAsync(result);

                        try 
                        {
                            using var doc = JsonDocument.Parse(responseBody);
                            if (doc.RootElement.TryGetProperty("Serial", out var serialElement))
                            {
                                var serialNumber = serialElement.GetString();
                                if (!string.IsNullOrEmpty(serialNumber))
                                {
                                    await _dbService.UpdateSensorSerialNumberAsync(sensor.Id, serialNumber);
                                }
                            }

                            await ProcessSensorDataAsync(sensor, doc, sessionId, cancellationToken);
                            Interlocked.Increment(ref successCount);
                        }
                        catch (Exception ex)
                        {
                            await _loggerService.LogAccessErrorAsync(sensor, ex, cancellationToken);
                            errors.Add($"Sensor {sensor.Id} ({sensor.EndPointsName ?? "Unknown"}): {ex.Message}");
                        }
                    }
                    else
                    {
                        var ex = exception ?? new Exception($"Polling failed with status {statusCode}");
                        await _loggerService.LogAccessErrorAsync(sensor, ex, cancellationToken);
                        errors.Add($"Sensor {sensor.Id} ({sensor.SensorType?.SensorTypeName ?? "Unknown"}): Status {statusCode}");
                    }
                }
                catch (Exception ex)
                {
                    await _loggerService.LogAccessErrorAsync(sensor, ex, cancellationToken);
                    errors.Add($"Sensor {sensor.Id} critical error: {ex.Message}");
                }
            });

            await Task.WhenAll(pollingTasks);
        }
        finally
        {
            string? errorJson = errors.Count > 0 ? JsonSerializer.Serialize(errors) : null;
            await _dbService.FinishPollingSessionAsync(sessionId, successCount, errorJson);
        }
    }

    public async Task ProcessSensorDataAsync(Sensor sensor, JsonDocument jsonDocument, Guid? pollingSessionId, CancellationToken cancellationToken)
    {
        var jsonString = jsonDocument.RootElement.GetRawText();
        switch (sensor.SensorType?.SensorTypeName)
        {
            case "DSPD": await ProcessDspdDataAsync(sensor, jsonString, pollingSessionId, cancellationToken); break;
            case "IWS": await ProcessIwsDataAsync(sensor, jsonString, pollingSessionId, cancellationToken); break;
            case "DOV": await ProcessDovDataAsync(sensor, jsonString, pollingSessionId, cancellationToken); break;
            case "MUEKS": await ProcessMueksDataAsync(sensor, jsonString, pollingSessionId, cancellationToken); break;
            case "DUST": await ProcessDustDataAsync(sensor, jsonString, pollingSessionId, cancellationToken); break;
        }
    }

    private async Task ProcessDspdDataAsync(Sensor sensor, string jsonString, Guid? pollingSessionId, CancellationToken cancellationToken)
    {
        try {
            var jsonModel = JsonSerializer.Deserialize<DspdJsonModel>(jsonString, _jsonOptions);
            if (jsonModel?.Packet == null) return;
            var p = jsonModel.Packet;
            var dataTimestamp = ParseDateTimeToUtc(p.DataTime, "dd-MM-yyyy,HH:mm:ss");
            var data = new DSPDData {
                SensorId = sensor.Id, ReceivedAt = DateTime.UtcNow, DataTimestamp = dataTimestamp,
                Grip = p.Grip, Shake = p.Shake, UPower = p.UPower, TemperatureCase = p.TemperatureCase,
                TemperatureRoad = p.TemperatureRoad, HeightH2O = p.HeightH2O, HeightIce = p.HeightIce,
                HeightSnow = p.HeightSnow, PercentICE = p.PercentICE, PercentPGM = p.PercentPGM,
                RoadStatus = p.RoadStatus, AngleToRoad = p.AngleToRoad, TemperatureFreezePGM = p.TemperatureFreezePGM,
                NeedCalibration = p.NeedCalibration, DistanceToSurface = p.DistanceToSurface,
                PollingSessionId = pollingSessionId, MonitoringPostId = sensor.MonitoringPostId
            };
            if (p.GPSLatitude != null && p.GPSLongitude != null) {
                var (lat, lon, valid) = ParseGpsCoordinates(p.GPSLatitude?.ToString(), p.GPSLongitude?.ToString());
                data.GPSLatitude = lat; data.GPSLongitude = lon; data.IsGpsValid = valid;
            }
            await _dbService.SaveDspdDataAsync(data, pollingSessionId, sensor.MonitoringPostId);
            
            await _dbService.UpdateDspdSensorCoordinatesAsync(sensor.Id, data.GPSLatitude, data.GPSLongitude);

            string sensorInfo = $"sensor '{sensor.SerialNumber}' ({sensor.EndPointsName})";
            string postInfo = sensor.MonitoringPost != null ? $" on post '{sensor.MonitoringPost.Name}'" : "";
            _logger.LogInformation("DSPD data saved for {sensorInfo}{postInfo} ({url})", 
                sensorInfo, postInfo, sensor.Url);
        } catch (Exception ex) { 
            _logger.LogError(ex, "DSPD parse error for sensor {sensorId}", sensor.Id);
            await _loggerService.LogDatabaseErrorAsync(sensor, "PARSE_ERROR", "DSPD JSON parse error: " + ex.Message, ex);
        }
    }

    private async Task ProcessIwsDataAsync(Sensor sensor, string jsonString, Guid? pollingSessionId, CancellationToken cancellationToken)
    {
        try {
            var jsonModel = JsonSerializer.Deserialize<IwsJsonModel>(jsonString, _jsonOptions);
            if (jsonModel?.Packet == null) return;
            var p = jsonModel.Packet;
            var dataTimestamp = ParseDateTimeToUtc(p.DataTime, "dd-MM-yyyy,HH:mm:ss.fff");
            var data = new IWSData {
                SensorId = sensor.Id, ReceivedAt = DateTime.UtcNow, DataTimestamp = dataTimestamp,
                EnvTemperature = p.EnvTemperature, Humidity = p.Humidity, DewPoint = p.DewPoint,
                PressureHPa = p.PressureHPa, PressureQNHHPa = p.PressureQNHHPa, PressureMmHg = p.PressureMmHg,
                WindSpeed = p.WindSpeed, WindDirection = p.WindDirection, WindVSound = p.WindVSound,
                PrecipitationType = p.PrecipitationType, PrecipitationIntensity = p.PrecipitationIntensity,
                PrecipitationQuantity = p.PrecipitationQuantity, PrecipitationElapsed = p.PrecipitationElapsed,
                PrecipitationPeriod = p.PrecipitationPeriod, CO2Level = p.CO2Level, SupplyVoltage = p.SupplyVoltage,
                Latitude = p.Latitude, Longitude = p.Longitude, Altitude = p.Altitude, KSP = p.KSP,
                GPSSpeed = p.GPSSpeed, AccelerationStDev = p.AccelerationStDev, Roll = p.Roll, Pitch = p.Pitch,
                WeAreFine = p.WeAreFine, PollingSessionId = pollingSessionId, MonitoringPostId = sensor.MonitoringPostId
            };
            await _dbService.SaveIwsDataAsync(data, pollingSessionId, sensor.MonitoringPostId);
            
            await _dbService.UpdateIwsSensorCoordinatesAsync(sensor.Id, (decimal?)data.Latitude, (decimal?)data.Longitude);

            string sensorInfo = $"sensor '{sensor.SerialNumber}' ({sensor.EndPointsName})";
            string postInfo = sensor.MonitoringPost != null ? $" on post '{sensor.MonitoringPost.Name}'" : "";
            _logger.LogInformation("IWS data saved for {sensorInfo}{postInfo} ({url})", 
                sensorInfo, postInfo, sensor.Url);
        } catch (Exception ex) { 
            _logger.LogError(ex, "IWS parse error for sensor {sensorId}", sensor.Id);
            await _loggerService.LogDatabaseErrorAsync(sensor, "PARSE_ERROR", "IWS JSON parse error: " + ex.Message, ex);
        }
    }

    private async Task ProcessMueksDataAsync(Sensor sensor, string jsonString, Guid? pollingSessionId, CancellationToken cancellationToken)
    {
        try {
            var jsonModel = JsonSerializer.Deserialize<MueksJsonModel>(jsonString, _jsonOptions);
            if (jsonModel?.Packet == null) return;
            var p = jsonModel.Packet;
            var dataTimestamp = ParseDateTimeToUtc(p.DataTime, "dd-MM-yyyy,HH:mm:ss");
            var data = new MUEKSData {
                SensorId = sensor.Id, ReceivedAt = DateTime.UtcNow, DataTimestamp = dataTimestamp,
                TemperatureBox = p.TemperatureBox, UPowerIn12B = p.UPowerIn12B, UOut12B = p.UOut12B,
                IOut12B = p.IOut12B, IOut48B = p.IOut48B, UAkb = p.UAkb, IAkb = p.IAkb,
                Sens220B = p.Sens220B, WhAkb = p.WhAkb, VisibleRange = p.VisibleRange, DoorStatus = p.DoorStatus,
                TdsH = p.TdsH, TdsTds = p.TdsTds, TkosaT1 = p.TkosaT1, TkosaT3 = p.TkosaT3,
                PollingSessionId = pollingSessionId, MonitoringPostId = sensor.MonitoringPostId
            };
            await _dbService.SaveMueksDataAsync(data, pollingSessionId, sensor.MonitoringPostId);

            string sensorInfo = $"sensor '{sensor.SerialNumber}' ({sensor.EndPointsName})";
            string postInfo = sensor.MonitoringPost != null ? $" on post '{sensor.MonitoringPost.Name}'" : "";
            _logger.LogInformation("MUEKS data saved for {sensorInfo}{postInfo} ({url})", 
                sensorInfo, postInfo, sensor.Url);
        } catch (Exception ex) { 
            _logger.LogError(ex, "MUEKS parse error for sensor {sensorId}", sensor.Id);
            await _loggerService.LogDatabaseErrorAsync(sensor, "PARSE_ERROR", "MUEKS JSON parse error: " + ex.Message, ex);
        }
    }

    private async Task ProcessDovDataAsync(Sensor sensor, string jsonString, Guid? pollingSessionId, CancellationToken cancellationToken)
    {
        try {
            var jsonModel = JsonSerializer.Deserialize<DovJsonModel>(jsonString, _jsonOptions);
            if (jsonModel?.Packet == null) return;
            var p = jsonModel.Packet;
            var data = new DOVData {
                SensorId = sensor.Id, ReceivedAt = DateTime.UtcNow, DataTimestamp = DateTime.UtcNow,
                VisibleRange = p.VisibleRange, BrightFlag = p.BrightFlag,
                PollingSessionId = pollingSessionId, MonitoringPostId = sensor.MonitoringPostId
            };
            await _dbService.SaveDovDataAsync(data, pollingSessionId, sensor.MonitoringPostId);

            string sensorInfo = $"sensor '{sensor.SerialNumber}' ({sensor.EndPointsName})";
            string postInfo = sensor.MonitoringPost != null ? $" on post '{sensor.MonitoringPost.Name}'" : "";
            _logger.LogInformation("DOV data saved for {sensorInfo}{postInfo} ({url})", 
                sensorInfo, postInfo, sensor.Url);
        } catch (Exception ex) { 
            _logger.LogError(ex, "DOV parse error for sensor {sensorId}", sensor.Id);
            await _loggerService.LogDatabaseErrorAsync(sensor, "PARSE_ERROR", "DOV JSON parse error: " + ex.Message, ex);
        }
    }

    private async Task ProcessDustDataAsync(Sensor sensor, string jsonString, Guid? pollingSessionId, CancellationToken cancellationToken)
    {
        try {
            var jsonModel = JsonSerializer.Deserialize<DustJsonModel>(jsonString, _jsonOptions);
            if (jsonModel?.Packet == null) return;
            var p = jsonModel.Packet;
            var data = new DUSTData {
                SensorId = sensor.Id, ReceivedAt = DateTime.UtcNow, DataTimestamp = DateTime.UtcNow,
                PM10Act = p.PM10_act, PM25Act = p.PM2_5_act, PM1Act = p.PM1_0_act,
                PM10AWG = p.PM10_awg, PM25AWG = p.PM2_5_awg, PM1AWG = p.PM1_0_awg,
                FlowProbe = p.Flow_probe, TemperatureProbe = p.Temperature_probe, HumidityProbe = p.Humidity_probe,
                LaserStatus = p.Laser_status, SupplyVoltage = p.Supply_voltage,
                PollingSessionId = pollingSessionId, MonitoringPostId = sensor.MonitoringPostId
            };
            await _dbService.SaveDustDataAsync(data, pollingSessionId, sensor.MonitoringPostId);

            string sensorInfo = $"sensor '{sensor.SerialNumber}' ({sensor.EndPointsName})";
            string postInfo = sensor.MonitoringPost != null ? $" on post '{sensor.MonitoringPost.Name}'" : "";
            _logger.LogInformation("DUST data saved for {sensorInfo}{postInfo} ({url})", 
                sensorInfo, postInfo, sensor.Url);
        } catch (Exception ex) { 
            _logger.LogError(ex, "DUST parse error for sensor {sensorId}", sensor.Id);
            await _loggerService.LogDatabaseErrorAsync(sensor, "PARSE_ERROR", "DUST JSON parse error: " + ex.Message, ex);
        }
    }

    private DateTime ParseDateTimeToUtc(string? dateTimeStr, string format)
    {
        if (string.IsNullOrEmpty(dateTimeStr)) return DateTime.UtcNow;
        if (DateTime.TryParseExact(dateTimeStr, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt))
            return DateTime.SpecifyKind(dt, DateTimeKind.Utc);
        return DateTime.UtcNow;
    }

    private (decimal? lat, decimal? lon, bool valid) ParseGpsCoordinates(string? latStr, string? lonStr)
    {
        if (string.IsNullOrEmpty(latStr) || string.IsNullOrEmpty(lonStr)) return (null, null, false);
        if (decimal.TryParse(latStr, NumberStyles.Any, CultureInfo.InvariantCulture, out var lat) &&
            decimal.TryParse(lonStr, NumberStyles.Any, CultureInfo.InvariantCulture, out var lon))
        {
            return (lat, lon, true);
        }
        return (null, null, false);
    }
}
