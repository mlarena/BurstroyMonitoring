using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BurstroyMonitoring.Data;
using BurstroyMonitoring.Data.Models;
using BurstroyMonitoring.Data.Models.ViewModels;
using Npgsql;

namespace BurstroyMonitoring.TCM.Controllers;

public class HomeController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<HomeController> _logger;

    public HomeController(ApplicationDbContext context, ILogger<HomeController> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IActionResult> Index()
    {
        try
        {
            var postsData = await _context.MonitoringPosts
                .OrderBy(mp => mp.Name)
                .Select(p => new
                {
                    Post = p,
                    TotalSensors = p.Sensors.Count(s => s.IsActive),
                    ErrorSensors = p.Sensors.Where(s => s.IsActive).Count(s => 
                        _context.SensorError.Any(e => e.SensorId == s.Id && 
                        (!_context.SensorResults.Any(r => r.SensorId == s.Id) || 
                         e.CreatedAt > _context.SensorResults.Where(r => r.SensorId == s.Id).Max(r => r.CheckedAt)))
                    )
                })
                .ToListAsync();

            // Преобразуем в список объектов, которые удобно использовать во View
            var viewModel = postsData.Select(d => {
                var p = d.Post;
                // Логика статуса:
                // 1. Если не активен -> inactive (красный)
                // 2. Если активен и есть ошибки, но не все -> warning (желтый)
                // 3. Если активен и все в ошибке -> danger (красный)
                // 4. Если все ок -> active (зеленый)
                
                string statusClass = "active";
                if (!p.IsActive) statusClass = "inactive";
                else if (d.TotalSensors > 0)
                {
                    if (d.ErrorSensors == d.TotalSensors) statusClass = "danger";
                    else if (d.ErrorSensors > 0) statusClass = "warning";
                }

                // Используем TempData или ViewBag для передачи классов, 
                // но лучше добавить свойство в модель или использовать анонимный тип.
                // Для простоты добавим информацию в ViewBag или расширим модель.
                return new { Post = p, StatusClass = statusClass, ErrorCount = d.ErrorSensors };
            }).ToList();

            ViewBag.PostStatuses = viewModel.ToDictionary(x => x.Post.Id, x => x.StatusClass);
            
            return View(postsData.Select(d => d.Post).ToList());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in HomeController.Index");
            return View(new List<MonitoringPost>());
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetPostSensors(int postId)
    {
        try
        {
            var sensorsData = await _context.Sensors
                .Include(s => s.SensorType)
                .Where(s => s.MonitoringPostId == postId && s.IsActive)
                .Select(s => new
                {
                    s.Id,
                    s.EndPointsName,
                    s.SerialNumber,
                    SensorTypeName = s.SensorType != null ? s.SensorType.SensorTypeName : "Unknown",
                    LastResult = _context.SensorResults
                        .Where(r => r.SensorId == s.Id)
                        .OrderByDescending(r => r.CheckedAt)
                        .Select(r => new { r.IsSuccess, r.CheckedAt })
                        .FirstOrDefault(),
                    LastError = _context.SensorError
                        .Where(e => e.SensorId == s.Id)
                        .OrderByDescending(e => e.CreatedAt)
                        .Select(e => new { e.ErrorMessage, e.CreatedAt })
                        .FirstOrDefault()
                })
                .ToListAsync();

            var result = sensorsData.Select(s =>
            {
                // Определяем, что считать ошибкой:
                // 1. Если в результатах явно стоит IsSuccess = false
                // 2. ИЛИ если в таблице ошибок есть запись, которая появилась ПОСЛЕ последней успешной проверки
                bool hasExplicitFailure = s.LastResult != null && !s.LastResult.IsSuccess;
                bool hasRecentError = s.LastError != null && (s.LastResult == null || s.LastError.CreatedAt > s.LastResult.CheckedAt);
                
                bool isSuccess = !hasExplicitFailure && !hasRecentError;

                return new
                {
                    s.Id,
                    s.EndPointsName,
                    s.SerialNumber,
                    s.SensorTypeName,
                    IsSuccess = isSuccess,
                    ErrorMessage = !isSuccess 
                        ? (s.LastError?.ErrorMessage ?? "Ошибка опроса") 
                        : null,
                    LastCheck = s.LastResult?.CheckedAt ?? s.LastError?.CreatedAt
                };
            });

            return Json(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetPostSensors for postId: {PostId}", postId);
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetLatestSensorData(int sensorId, string type)
    {
        try
        {
            var connectionString = _context.Database.GetConnectionString();
            object? data = null;

            using var connection = new NpgsqlConnection(connectionString);
            await connection.OpenAsync();

            switch (type.ToUpper())
            {
                case "IWS":
                    data = await GetLatestIws(connection, sensorId);
                    break;
                case "DSPD":
                    data = await GetLatestDspd(connection, sensorId);
                    break;
                case "DOV":
                    data = await GetLatestDov(connection, sensorId);
                    break;
                case "DUST":
                    data = await GetLatestDust(connection, sensorId);
                    break;
                case "MUEKS":
                    data = await GetLatestMueks(connection, sensorId);
                    break;
            }

            if (data != null)
            {
                _logger.LogDebug("Returned data for sensor {SensorId} ({Type})", sensorId, type);
            }
            else
            {
                _logger.LogDebug("No data found for sensor {SensorId} ({Type})", sensorId, type);
            }

            return Json(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting data for sensor {SensorId} ({Type})", sensorId, type);
            return StatusCode(500, new { error = ex.Message });
        }
    }

    private async Task<IWSMeasurementViewModel?> GetLatestIws(NpgsqlConnection conn, int sensorId)
    {
        var query = "SELECT * FROM public.vw_iws_data_full WHERE sensor_id = @sid ORDER BY received_at DESC LIMIT 1";
        using var cmd = new NpgsqlCommand(query, conn);
        cmd.Parameters.AddWithValue("@sid", sensorId);
        using var reader = await cmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return new IWSMeasurementViewModel
            {
                ReceivedAt = reader.GetDateTime(reader.GetOrdinal("received_at")),
                EnvironmentTemperature = GetDecimal(reader, "environment_temperature"),
                HumidityPercentage = GetDecimal(reader, "humidity_percentage"),
                DewPoint = GetDecimal(reader, "dew_point"),
                PressureHpa = GetDecimal(reader, "pressure_hpa"),
                PressureQNHHpa = GetDecimal(reader, "pressure_qnh_hpa"),
                PressureMmHg = GetDecimal(reader, "pressure_mmhg"),
                WindSpeed = GetDecimal(reader, "wind_speed"),
                WindDirection = GetDecimal(reader, "wind_direction"),
                WindVSound = GetDecimal(reader, "wind_vs_sound"),
                PrecipitationType = GetInt(reader, "precipitation_type"),
                PrecipitationIntensity = GetDecimal(reader, "precipitation_intensity"),
                PrecipitationQuantity = GetDecimal(reader, "precipitation_quantity"),
                PrecipitationElapsed = GetInt(reader, "precipitation_elapsed"),
                PrecipitationPeriod = GetInt(reader, "precipitation_period"),
                Co2Level = GetDecimal(reader, "co2_level")
            };
        }
        return null;
    }

    private async Task<DSPDMeasurementViewModel?> GetLatestDspd(NpgsqlConnection conn, int sensorId)
    {
        var query = "SELECT * FROM public.vw_dspd_data_full WHERE sensor_id = @sid ORDER BY received_at DESC LIMIT 1";
        using var cmd = new NpgsqlCommand(query, conn);
        cmd.Parameters.AddWithValue("@sid", sensorId);
        using var reader = await cmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return new DSPDMeasurementViewModel
            {
                ReceivedAt = reader.GetDateTime(reader.GetOrdinal("received_at")),
                GripCoefficient = GetDecimal(reader, "grip_coefficient"),
                RoadTemperature = GetDecimal(reader, "road_temperature"),
                WaterHeight = GetDecimal(reader, "water_height"),
                IceHeight = GetDecimal(reader, "ice_height"),
                SnowHeight = GetDecimal(reader, "snow_height"),
                IcePercentage = GetDecimal(reader, "ice_percentage"),
                PgmPercentage = GetDecimal(reader, "pgm_percentage"),
                RoadStatusCode = GetInt(reader, "road_status_code"),
                FreezeTemperature = GetDecimal(reader, "freeze_temperature")
            };
        }
        return null;
    }

    private async Task<DOVMeasurementViewModel?> GetLatestDov(NpgsqlConnection conn, int sensorId)
    {
        var query = "SELECT * FROM public.vw_dov_data_full WHERE sensor_id = @sid ORDER BY received_at DESC LIMIT 1";
        using var cmd = new NpgsqlCommand(query, conn);
        cmd.Parameters.AddWithValue("@sid", sensorId);
        using var reader = await cmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return new DOVMeasurementViewModel
            {
                ReceivedAt = reader.GetDateTime(reader.GetOrdinal("received_at")),
                VisibleRange = reader.GetDecimal(reader.GetOrdinal("visible_range"))
            };
        }
        return null;
    }

    private async Task<DUSTMeasurementViewModel?> GetLatestDust(NpgsqlConnection conn, int sensorId)
    {
        var query = "SELECT * FROM public.vw_dust_data_full WHERE sensor_id = @sid ORDER BY received_at DESC LIMIT 1";
        using var cmd = new NpgsqlCommand(query, conn);
        cmd.Parameters.AddWithValue("@sid", sensorId);
        using var reader = await cmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return new DUSTMeasurementViewModel
            {
                ReceivedAt = reader.GetDateTime(reader.GetOrdinal("received_at")),
                Pm10Act = GetDecimal(reader, "pm10act"),
                Pm25Act = GetDecimal(reader, "pm25act"),
                Pm1Act = GetDecimal(reader, "pm1act"),
                Pm10Awg = GetDecimal(reader, "pm10awg"),
                Pm25Awg = GetDecimal(reader, "pm25awg"),
                Pm1Awg = GetDecimal(reader, "pm1awg")
            };
        }
        return null;
    }

    private async Task<MUEKSMeasurementViewModel?> GetLatestMueks(NpgsqlConnection conn, int sensorId)
    {
        var query = "SELECT * FROM public.vw_mueks_data_full WHERE sensor_id = @sid ORDER BY received_at DESC LIMIT 1";
        using var cmd = new NpgsqlCommand(query, conn);
        cmd.Parameters.AddWithValue("@sid", sensorId);
        using var reader = await cmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return new MUEKSMeasurementViewModel
            {
                ReceivedAt = reader.GetDateTime(reader.GetOrdinal("received_at")),
                TemperatureBox = GetDecimal(reader, "temperature_box"),
                VoltagePowerIn12b = GetDecimal(reader, "voltage_power_in_12b"),
                VoltageOut12b = GetDecimal(reader, "voltage_out_12b"),
                VoltageAkb = GetDecimal(reader, "voltage_akb"),
                CurrentOut12b = GetDecimal(reader, "current_out_12b"),
                CurrentOut48b = GetDecimal(reader, "current_out_48b"),
                CurrentAkb = GetDecimal(reader, "current_akb"),
                WattHoursAkb = GetDecimal(reader, "watt_hours_akb"),
                VisibleRange = GetDecimal(reader, "visible_range"),
                Sensor220b = GetInt(reader, "sensor_220b"),
                OwenCh1 = GetDecimal(reader, "owen_ch1"),
                OwenCh2 = GetDecimal(reader, "owen_ch2")
            };        }
        return null;
    }

    private decimal? GetDecimal(NpgsqlDataReader reader, string column)
    {
        var ordinal = reader.GetOrdinal(column);
        return reader.IsDBNull(ordinal) ? null : reader.GetDecimal(ordinal);
    }

    private int? GetInt(NpgsqlDataReader reader, string column)
    {
        var ordinal = reader.GetOrdinal(column);
        return reader.IsDBNull(ordinal) ? null : reader.GetInt32(ordinal);
    }
}
