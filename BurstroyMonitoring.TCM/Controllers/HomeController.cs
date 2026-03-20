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

    public HomeController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var posts = await _context.MonitoringPosts
            .OrderBy(mp => mp.Name)
            .ToListAsync();
        return View(posts);
    }

    [HttpGet]
    public async Task<IActionResult> GetPostSensors(int postId)
    {
        var sensors = await _context.Sensors
            .Include(s => s.SensorType)
            .Where(s => s.MonitoringPostId == postId && s.IsActive)
            .Select(s => new
            {
                s.Id,
                s.EndPointsName,
                s.SerialNumber,
                SensorTypeName = s.SensorType != null ? s.SensorType.SensorTypeName : "Unknown"
            })
            .ToListAsync();

        return Json(sensors);
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
                Console.WriteLine($"[HomeController] Returned data for sensor {sensorId} ({type})");
            }
            else
            {
                Console.WriteLine($"[HomeController] No data found for sensor {sensorId} ({type})");
            }

            return Json(data);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[HomeController] Error getting data for sensor {sensorId}: {ex.Message}");
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
                Sensor220b = GetInt(reader, "sensor_220b")
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
