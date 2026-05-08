using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BurstroyMonitoring.Data;
using BurstroyMonitoring.Data.Models;
using Npgsql;
using BurstroyMonitoring.Data.Models.ViewModels;

namespace BurstroyMonitoring.TCM.Controllers;

public class PoverkaController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<PoverkaController> _logger;

    public PoverkaController(ApplicationDbContext context, ILogger<PoverkaController> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IActionResult> Index(int? postId)
    {
        try
        {
            var posts = await _context.MonitoringPosts
                .OrderBy(p => p.Name)
                .ToListAsync();

            ViewBag.Posts = posts;

            var query = _context.Sensors
                .Include(s => s.MonitoringPost)
                .Include(s => s.SensorType)
                .Where(s => s.IsActive);

            if (postId.HasValue)
            {
                query = query.Where(s => s.MonitoringPostId == postId.Value);
                ViewBag.SelectedPostId = postId.Value;
            }
            else if (posts.Any())
            {
                var firstPostId = posts.First().Id;
                query = query.Where(s => s.MonitoringPostId == firstPostId);
                ViewBag.SelectedPostId = firstPostId;
            }

            var sensors = await query
                .OrderBy(s => s.EndPointsName)
                .ToListAsync();

            return View(sensors);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in PoverkaController.Index");
            return View(new List<Sensor>());
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetAllSensorsData(int postId)
    {
        try
        {
            var sensors = await _context.Sensors
                .Where(s => s.MonitoringPostId == postId && s.IsActive)
                .Include(s => s.SensorType)
                .ToListAsync();

            var connectionString = _context.Database.GetConnectionString();
            var results = new Dictionary<int, object?>();

            using var connection = new NpgsqlConnection(connectionString);
            await connection.OpenAsync();

            foreach (var sensor in sensors)
            {
                var type = sensor.SensorType?.SensorTypeName?.ToUpper();
                object? data = type switch
                {
                    "IWS" => await GetLatestIws(connection, sensor.Id),
                    "DSPD" => await GetLatestDspd(connection, sensor.Id),
                    "DOV" => await GetLatestDov(connection, sensor.Id),
                    "DUST" => await GetLatestDust(connection, sensor.Id),
                    "MUEKS" => await GetLatestMueks(connection, sensor.Id),
                    _ => null
                };
                results[sensor.Id] = data;
            }

            return Json(results);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all sensors data for post {PostId}", postId);
            return StatusCode(500, new { error = ex.Message });
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
                PressureHpa = GetDecimal(reader, "pressure_hpa"),
                PressureMmHg = GetDecimal(reader, "pressure_mmhg"),
                WindSpeed = GetDecimal(reader, "wind_speed"),
                WindDirection = GetDecimal(reader, "wind_direction"),
                PrecipitationIntensity = GetDecimal(reader, "precipitation_intensity"),
                PrecipitationQuantity = GetDecimal(reader, "precipitation_quantity"),
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
                RoadTemperature = GetDecimal(reader, "road_temperature"),
                RoadStatusCode = GetInt(reader, "road_status_code"),
                WaterHeight = GetDecimal(reader, "water_height"),
                IceHeight = GetDecimal(reader, "ice_height"),
                SnowHeight = GetDecimal(reader, "snow_height")
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
                Pm1Act = GetDecimal(reader, "pm1act")
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
                OwenCh1 = GetDecimal(reader, "owen_ch1")
            };
        }
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
