using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using BurstroyMonitoring.Data;
using BurstroyMonitoring.Data.Models;
using BurstroyMonitoring.Data.Models.ViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Text.Json;

namespace GraphsAndChartsApp.Controllers
{
    public class GraphsAndChartsController : Controller
    {
        private readonly ApplicationDbContext _context;
        public GraphsAndChartsController(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index()
        {
            var viewModel = new SensorSelectionViewModel();
            var monitoringPosts = await _context.MonitoringPosts
                .Where(mp => mp.IsActive)
                .OrderBy(mp => mp.Name)
                .ToListAsync();
            viewModel.MonitoringPosts = monitoringPosts
                .Select(mp => new SelectListItem
                {
                    Value = mp.Id.ToString(),
                    Text = mp.Name
                })
                .ToList();
            viewModel.MonitoringPosts.Insert(0, new SelectListItem
            {
                Value = "",
                Text = "Выберите пост мониторинга"
            });
            viewModel.Sensors = new List<SelectListItem>
            {
                new SelectListItem
                {
                    Value = "",
                    Text = "Сначала выберите пост мониторинга"
                }
            };
            return View(viewModel);
        }
        [HttpGet]
        public async Task<IActionResult> GetSensorsByPost(int monitoringPostId)
        {
            var sensors = await _context.Sensors
                .Include(s => s.SensorType)
                .Where(s => s.MonitoringPostId == monitoringPostId && s.IsActive)
                .OrderBy(s => s.SensorType != null ? s.SensorType.SensorTypeName : "")
                .ThenBy(s => s.EndPointsName)
                .ToListAsync();
            var sensorItems = sensors
                .Select(s => new SelectListItem
                {
                    Value = s.Id.ToString(),
                    Text = $"{s.SensorType?.SensorTypeName} {s.EndPointsName} {s.SerialNumber}"
                })
                .ToList();
            sensorItems.Insert(0, new SelectListItem
            {
                Value = "",
                Text = "Выберите сенсор"
            });
            return Json(sensorItems);
        }
        [HttpGet]
        public async Task<IActionResult> GetSensorData(int sensorId)
        {
            var sensor = await _context.Sensors
                .Include(s => s.SensorType)
                .Include(s => s.MonitoringPost)
                .FirstOrDefaultAsync(s => s.Id == sensorId);
            if (sensor == null || sensor.SensorType == null)
            {
                return NotFound();
            }
            var viewModel = new SensorViewModel
            {
                Id = sensor.Id,
                SensorTypeName = sensor.SensorType.SensorTypeName,
                EndPointsName = sensor.EndPointsName,
                SerialNumber = sensor.SerialNumber,
                MonitoringPostName = sensor.MonitoringPost?.Name
            };
            return sensor.SensorType.SensorTypeName switch
            {
                "DSPD" => PartialView("_DSPDPartial", viewModel),
                "IWS" => PartialView("_IWSPartial", viewModel),
                "DOV" => PartialView("_DOVPartial", viewModel),
                "DUST" => PartialView("_DUSTPartial", viewModel),
                "MUEKS" => PartialView("_MUEKSPartial", viewModel),
                _ => PartialView("_DefaultPartial", viewModel)
            };
        }


        
#region Данные все которые есть в таблице

        [HttpGet]
        public async Task<IActionResult> GetDOVData(int sensorId, int days = 1)
        {
            try
            {
                var fromDate = DateTime.UtcNow.AddDays(-days);
               
                var connectionString = _context.Database.GetConnectionString();
               
                var query = @"
                    SELECT
                        received_at AS ReceivedAt,
                        visible_range AS VisibleRange
                    FROM public.vw_dov_data_full
                    WHERE sensor_id = @sensorId
                        AND received_at >= @fromDate
                    ORDER BY received_at ASC";
               
                var measurements = new List<DOVMeasurementViewModel>();
               
                using (var connection = new NpgsqlConnection(connectionString))
                {
                    using (var command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@sensorId", sensorId);
                        command.Parameters.AddWithValue("@fromDate", fromDate);
                       
                        await connection.OpenAsync();
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                measurements.Add(new DOVMeasurementViewModel
                                {
                                    ReceivedAt = reader.GetDateTime(reader.GetOrdinal("ReceivedAt")),
                                    VisibleRange = reader.GetDecimal(reader.GetOrdinal("VisibleRange"))
                                });
                            }
                        }
                    }
                }
                var sensor = await _context.Sensors
                    .Include(s => s.SensorType)
                    .Include(s => s.MonitoringPost)
                    .FirstOrDefaultAsync(s => s.Id == sensorId);
                var viewModel = new DOVDataViewModel
                {
                    SensorId = sensorId,
                    SerialNumber = sensor?.SerialNumber ?? "",
                    EndpointName = sensor?.EndPointsName ?? "",
                    PostName = sensor?.MonitoringPost?.Name,
                    Measurements = measurements
                };
                Console.WriteLine(Json(viewModel).ToString());
                return Json(viewModel);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при загрузке данных DOV: {ex.Message}");
                return StatusCode(500, new { error = "Ошибка при загрузке данных" });
            }
        }
       
        [HttpGet]
        public async Task<IActionResult> GetDSPDData(int sensorId, int days = 1)
        {
            try
            {
                var fromDate = DateTime.UtcNow.AddDays(-days);
               
                var connectionString = _context.Database.GetConnectionString();
               
                var query = @"
                    SELECT
                        received_at AS ReceivedAt,
                        grip_coefficient AS GripCoefficient,
                        shake_level AS ShakeLevel,
                        voltage_power AS VoltagePower,
                        case_temperature AS CaseTemperature,
                        road_temperature AS RoadTemperature,
                        water_height AS WaterHeight,
                        ice_height AS IceHeight,
                        snow_height AS SnowHeight,
                        ice_percentage AS IcePercentage,
                        pgm_percentage AS PgmPercentage,
                        road_status_code AS RoadStatusCode,
                        road_angle AS RoadAngle,
                        freeze_temperature AS FreezeTemperature,
                        distance_to_surface AS DistanceToSurface
                    FROM public.vw_dspd_data_full
                    WHERE sensor_id = @sensorId
                        AND received_at >= @fromDate
                    ORDER BY received_at ASC";
               
                var measurements = new List<DSPDMeasurementViewModel>();
               
                using (var connection = new NpgsqlConnection(connectionString))
                {
                    using (var command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@sensorId", sensorId);
                        command.Parameters.AddWithValue("@fromDate", fromDate);
                       
                        await connection.OpenAsync();
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var measurement = new DSPDMeasurementViewModel
                                {
                                    ReceivedAt = reader.GetDateTime(reader.GetOrdinal("ReceivedAt")),
                                    GripCoefficient = GetDecimalOrNull(reader, "GripCoefficient"),
                                    ShakeLevel = GetDecimalOrNull(reader, "ShakeLevel"),
                                    VoltagePower = GetDecimalOrNull(reader, "VoltagePower"),
                                    CaseTemperature = GetDecimalOrNull(reader, "CaseTemperature"),
                                    RoadTemperature = GetDecimalOrNull(reader, "RoadTemperature"),
                                    WaterHeight = GetDecimalOrNull(reader, "WaterHeight"),
                                    IceHeight = GetDecimalOrNull(reader, "IceHeight"),
                                    SnowHeight = GetDecimalOrNull(reader, "SnowHeight"),
                                    IcePercentage = GetDecimalOrNull(reader, "IcePercentage"),
                                    PgmPercentage = GetDecimalOrNull(reader, "PgmPercentage"),
                                    RoadStatusCode = GetInt32OrNull(reader, "RoadStatusCode"),
                                    RoadAngle = GetDecimalOrNull(reader, "RoadAngle"),
                                    FreezeTemperature = GetDecimalOrNull(reader, "FreezeTemperature"),
                                    DistanceToSurface = GetDecimalOrNull(reader, "DistanceToSurface")
                                };
                               
                                measurements.Add(measurement);
                            }
                        }
                    }
                }
                var sensor = await _context.Sensors
                    .Include(s => s.SensorType)
                    .Include(s => s.MonitoringPost)
                    .FirstOrDefaultAsync(s => s.Id == sensorId);
                var viewModel = new DSPDDataViewModel
                {
                    SensorId = sensorId,
                    SerialNumber = sensor?.SerialNumber ?? "",
                    EndpointName = sensor?.EndPointsName ?? "",
                    PostName = sensor?.MonitoringPost?.Name,
                    Measurements = measurements
                };
                return Json(viewModel);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при загрузке данных DSPD: {ex.Message}");
                return StatusCode(500, new { error = "Ошибка при загрузке данных" });
            }
        }
        
        [HttpGet]
        public async Task<IActionResult> GetDUSTData(int sensorId, int days = 1)
        {
            try
            {
                var fromDate = DateTime.UtcNow.AddDays(-days);
               
                var connectionString = _context.Database.GetConnectionString();
               
                var query = @"
                    SELECT
                        received_at AS ReceivedAt,
                        pm10act AS Pm10Act,
                        pm25act AS Pm25Act,
                        pm1act AS Pm1Act,
                        pm10awg AS Pm10Awg,
                        pm25awg AS Pm25Awg,
                        pm1awg AS Pm1Awg,
                        flowprobe AS FlowProbe,
                        temperatureprobe AS TemperatureProbe,
                        humidityprobe AS HumidityProbe,
                        laserstatus AS LaserStatus,
                        supplyvoltage AS SupplyVoltage
                    FROM public.vw_dust_data_full
                    WHERE sensor_id = @sensorId
                        AND received_at >= @fromDate
                    ORDER BY received_at ASC";
               
                var measurements = new List<DUSTMeasurementViewModel>();
               
                using (var connection = new NpgsqlConnection(connectionString))
                {
                    using (var command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@sensorId", sensorId);
                        command.Parameters.AddWithValue("@fromDate", fromDate);
                       
                        await connection.OpenAsync();
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var measurement = new DUSTMeasurementViewModel
                                {
                                    ReceivedAt = reader.GetDateTime(reader.GetOrdinal("ReceivedAt")),
                                    Pm10Act = GetDecimalOrNull(reader, "Pm10Act"),
                                    Pm25Act = GetDecimalOrNull(reader, "Pm25Act"),
                                    Pm1Act = GetDecimalOrNull(reader, "Pm1Act"),
                                    Pm10Awg = GetDecimalOrNull(reader, "Pm10Awg"),
                                    Pm25Awg = GetDecimalOrNull(reader, "Pm25Awg"),
                                    Pm1Awg = GetDecimalOrNull(reader, "Pm1Awg"),
                                    FlowProbe = GetDecimalOrNull(reader, "FlowProbe"),
                                    TemperatureProbe = GetDecimalOrNull(reader, "TemperatureProbe")
                                };
                               
                                measurements.Add(measurement);
                            }
                        }
                    }
                }
                var sensor = await _context.Sensors
                    .Include(s => s.SensorType)
                    .Include(s => s.MonitoringPost)
                    .FirstOrDefaultAsync(s => s.Id == sensorId);
                var viewModel = new DUSTDataViewModel
                {
                    SensorId = sensorId,
                    SerialNumber = sensor?.SerialNumber ?? "",
                    EndpointName = sensor?.EndPointsName ?? "",
                    PostName = sensor?.MonitoringPost?.Name,
                    Measurements = measurements
                };
                return Json(viewModel);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при загрузке данных DUST: {ex.Message}");
                return StatusCode(500, new { error = "Ошибка при загрузке данных" });
            }
        }
      
        [HttpGet]
        public async Task<IActionResult> GetIWSData(int sensorId, int days = 1)
        {
            try
            {
                var fromDate = DateTime.UtcNow.AddDays(-days);
               
                var connectionString = _context.Database.GetConnectionString();
               
                var query = @"
                    SELECT
                        received_at AS ReceivedAt,
                        environment_temperature AS EnvironmentTemperature,
                        humidity_percentage AS HumidityPercentage,
                        dew_point AS DewPoint,
                        pressure_hpa AS PressureHpa,
                        pressure_qnh_hpa AS PressureQNHHpa,
                        pressure_mmhg AS PressureMmHg,
                        wind_speed AS WindSpeed,
                        wind_direction AS WindDirection,
                        wind_vs_sound AS WindVSound,
                        precipitation_type AS PrecipitationType,
                        precipitation_intensity AS PrecipitationIntensity,
                        precipitation_quantity AS PrecipitationQuantity,
                        precipitation_elapsed AS PrecipitationElapsed,
                        precipitation_period AS PrecipitationPeriod,
                        co2_level AS Co2Level,
                        supply_voltage AS SupplyVoltage,
                        altitude AS Altitude,
                        ksp_value AS KspValue,
                        gps_speed AS GpsSpeed,
                        acceleration_std_dev AS AccelerationStdDev,
                        roll_angle AS RollAngle,
                        pitch_angle AS PitchAngle
                    FROM public.vw_iws_data_full
                    WHERE sensor_id = @sensorId
                        AND received_at >= @fromDate
                    ORDER BY received_at ASC";
               
                var measurements = new List<IWSMeasurementViewModel>();
               
                using (var connection = new NpgsqlConnection(connectionString))
                {
                    using (var command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@sensorId", sensorId);
                        command.Parameters.AddWithValue("@fromDate", fromDate);
                       
                        await connection.OpenAsync();
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var measurement = new IWSMeasurementViewModel
                                {
                                    ReceivedAt = reader.GetDateTime(reader.GetOrdinal("ReceivedAt")),
                                   
                                    EnvironmentTemperature = GetDecimalOrNull(reader, "EnvironmentTemperature"),
                                    HumidityPercentage = GetDecimalOrNull(reader, "HumidityPercentage"),
                                    DewPoint = GetDecimalOrNull(reader, "DewPoint"),
                                   
                                    PressureHpa = GetDecimalOrNull(reader, "PressureHpa"),
                                    PressureQNHHpa = GetDecimalOrNull(reader, "PressureQNHHpa"),
                                    PressureMmHg = GetDecimalOrNull(reader, "PressureMmHg"),
                                   
                                    WindSpeed = GetDecimalOrNull(reader, "WindSpeed"),
                                    WindDirection = GetDecimalOrNull(reader, "WindDirection"),
                                    WindVSound = GetDecimalOrNull(reader, "WindVSound"),
                                   
                                    PrecipitationType = GetInt32OrNull(reader, "PrecipitationType"),
                                    PrecipitationIntensity = GetDecimalOrNull(reader, "PrecipitationIntensity"),
                                    PrecipitationQuantity = GetDecimalOrNull(reader, "PrecipitationQuantity"),
                                    PrecipitationElapsed = GetInt32OrNull(reader, "PrecipitationElapsed"),
                                    PrecipitationPeriod = GetInt32OrNull(reader, "PrecipitationPeriod"),
                                   
                                    Co2Level = GetDecimalOrNull(reader, "Co2Level"),
                                    SupplyVoltage = GetDecimalOrNull(reader, "SupplyVoltage"),
                                   
                                    IwsLatitude = GetDecimalOrNull(reader, "IwsLatitude"),
                                    IwsLongitude = GetDecimalOrNull(reader, "IwsLongitude"),
                                    Altitude = GetDecimalOrNull(reader, "Altitude"),
                                   
                                    KspValue = GetInt32OrNull(reader, "KspValue"),
                                    GpsSpeed = GetDecimalOrNull(reader, "GpsSpeed"),
                                    AccelerationStdDev = GetDecimalOrNull(reader, "AccelerationStdDev"),
                                    RollAngle = GetDecimalOrNull(reader, "RollAngle"),
                                    PitchAngle = GetDecimalOrNull(reader, "PitchAngle")
                                };
                               
                                measurements.Add(measurement);
                            }
                        }
                    }
                }
                var sensor = await _context.Sensors
                    .Include(s => s.SensorType)
                    .Include(s => s.MonitoringPost)
                    .FirstOrDefaultAsync(s => s.Id == sensorId);
                var viewModel = new IWSDataViewModel
                {
                    SensorId = sensorId,
                    SerialNumber = sensor?.SerialNumber ?? "",
                    EndpointName = sensor?.EndPointsName ?? "",
                    PostName = sensor?.MonitoringPost?.Name,
                    Measurements = measurements
                };
                return Json(viewModel);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при загрузке данных IWS: {ex.Message}");
                return StatusCode(500, new { error = "Ошибка при загрузке данных" });
            }
        }
       
        [HttpGet]
        public async Task<IActionResult> GetMUEKSData(int sensorId, int days = 1)
        {
            try
            {
                var fromDate = DateTime.UtcNow.AddDays(-days);
               
                var connectionString = _context.Database.GetConnectionString();
               
                var query = @"
                    SELECT
                        received_at AS ReceivedAt,
                        temperature_box AS TemperatureBox,
                        voltage_power_in_12b AS VoltagePowerIn12b,
                        voltage_out_12b AS VoltageOut12b,
                        current_out_12b AS CurrentOut12b,
                        current_out_48b AS CurrentOut48b,
                        voltage_akb AS VoltageAkb,
                        current_akb AS CurrentAkb,
                        sensor_220b AS Sensor220b,
                        watt_hours_akb AS WattHoursAkb,
                        visible_range AS VisibleRange,
                        door_status AS DoorStatus,
                        tds_h AS TdsH,
                        tds_tds AS TdsTds,
                        tkosa_t1 AS TkosaT1,
                        tkosa_t3 AS TkosaT3
                    FROM public.vw_mueks_data_full
                    WHERE sensor_id = @sensorId
                        AND received_at >= @fromDate
                    ORDER BY received_at ASC";
               
                var measurements = new List<MUEKSMeasurementViewModel>();
               
                using (var connection = new NpgsqlConnection(connectionString))
                {
                    using (var command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@sensorId", sensorId);
                        command.Parameters.AddWithValue("@fromDate", fromDate);
                       
                        await connection.OpenAsync();
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var measurement = new MUEKSMeasurementViewModel
                                {
                                    ReceivedAt = reader.GetDateTime(reader.GetOrdinal("ReceivedAt")),
                                   
                                    TemperatureBox = GetDecimalOrNull(reader, "TemperatureBox"),
                                    VoltagePowerIn12b = GetDecimalOrNull(reader, "VoltagePowerIn12b"),
                                    VoltageOut12b = GetDecimalOrNull(reader, "VoltageOut12b"),
                                    CurrentOut12b = GetDecimalOrNull(reader, "CurrentOut12b"),
                                    CurrentOut48b = GetDecimalOrNull(reader, "CurrentOut48b"),
                                    VoltageAkb = GetDecimalOrNull(reader, "VoltageAkb"),
                                    CurrentAkb = GetDecimalOrNull(reader, "CurrentAkb"),
                                    Sensor220b = GetInt32OrNull(reader, "Sensor220b"),
                                    WattHoursAkb = GetDecimalOrNull(reader, "WattHoursAkb"),
                                    VisibleRange = GetDecimalOrNull(reader, "VisibleRange"),
                                    DoorStatus = GetInt32OrNull(reader, "DoorStatus"),
                                   
                                    TdsH = GetStringOrNull(reader, "TdsH"),
                                    TdsTds = GetStringOrNull(reader, "TdsTds"),
                                    TkosaT1 = GetStringOrNull(reader, "TkosaT1"),
                                    TkosaT3 = GetStringOrNull(reader, "TkosaT3")
                                };
                               
                                measurements.Add(measurement);
                            }
                        }
                    }
                }
                var sensor = await _context.Sensors
                    .Include(s => s.SensorType)
                    .Include(s => s.MonitoringPost)
                    .FirstOrDefaultAsync(s => s.Id == sensorId);
                var viewModel = new MUEKSDataViewModel
                {
                    SensorId = sensorId,
                    SerialNumber = sensor?.SerialNumber ?? "",
                    EndpointName = sensor?.EndPointsName ?? "",
                    PostName = sensor?.MonitoringPost?.Name,
                    Measurements = measurements
                };
                return Json(viewModel);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при загрузке данных MUEKS: {ex.Message}");
                return StatusCode(500, new { error = "Ошибка при загрузке данных" });
            }
        }
        
#endregion


#region Данные сгруппированные по часам

       [HttpGet]
        public async Task<IActionResult> GetDOVDataHour(int sensorId, int days = 1)
        {
            try
            {
                var fromDate = DateTime.UtcNow.AddDays(-days);
                
                var connectionString = _context.Database.GetConnectionString();
                
                var query = @"
                    SELECT 
                        DATE_TRUNC('hour', received_at) AS ReceivedAt,
                        ROUND(AVG(visible_range)::numeric, 3) AS VisibleRange
                    FROM public.vw_dov_data_full
                    WHERE sensor_id = @sensorId
                        AND received_at >= @fromDate
                    GROUP BY DATE_TRUNC('hour', received_at)
                    ORDER BY ReceivedAt ASC";
                
                var measurements = new List<DOVMeasurementViewModel>();
                
                using (var connection = new NpgsqlConnection(connectionString))
                {
                    using (var command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@sensorId", sensorId);
                        command.Parameters.AddWithValue("@fromDate", fromDate);
                        
                        await connection.OpenAsync();
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                measurements.Add(new DOVMeasurementViewModel
                                {
                                    ReceivedAt = reader.GetDateTime(reader.GetOrdinal("ReceivedAt")),
                                    VisibleRange = reader.GetDecimal(reader.GetOrdinal("VisibleRange"))
                                });
                            }
                        }
                    }
                }
                
                var sensor = await _context.Sensors
                    .Include(s => s.SensorType)
                    .Include(s => s.MonitoringPost)
                    .FirstOrDefaultAsync(s => s.Id == sensorId);
                
                var viewModel = new DOVDataViewModel
                {
                    SensorId = sensorId,
                    SerialNumber = sensor?.SerialNumber ?? "",
                    EndpointName = sensor?.EndPointsName ?? "",
                    PostName = sensor?.MonitoringPost?.Name,
                    Measurements = measurements
                };
                
                Console.WriteLine(Json(viewModel).ToString());
                return Json(viewModel);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при загрузке почасовых данных DOV: {ex.Message}");
                return StatusCode(500, new { error = "Ошибка при загрузке почасовых данных" });
            }
        }
       
        
        [HttpGet]
        public async Task<IActionResult> GetDSPDDataHour(int sensorId, int days = 1)
        {
            try
            {
                var fromDate = DateTime.UtcNow.AddDays(-days);
                
                var connectionString = _context.Database.GetConnectionString();
                
                var query = @"
                    SELECT 
                        DATE_TRUNC('hour', received_at) AS ReceivedAt,
                        ROUND(AVG(grip_coefficient)::numeric, 3) AS GripCoefficient,
                        ROUND(AVG(shake_level)::numeric, 3) AS ShakeLevel,
                        ROUND(AVG(voltage_power)::numeric, 3) AS VoltagePower,
                        ROUND(AVG(case_temperature)::numeric, 3) AS CaseTemperature,
                        ROUND(AVG(road_temperature)::numeric, 3) AS RoadTemperature,
                        ROUND(AVG(water_height)::numeric, 3) AS WaterHeight,
                        ROUND(AVG(ice_height)::numeric, 3) AS IceHeight,
                        ROUND(AVG(snow_height)::numeric, 3) AS SnowHeight,
                        ROUND(AVG(ice_percentage)::numeric, 3) AS IcePercentage,
                        ROUND(AVG(pgm_percentage)::numeric, 3) AS PgmPercentage,
                        AVG(road_status_code) AS RoadStatusCode,
                        ROUND(AVG(road_angle)::numeric, 3) AS RoadAngle,
                        ROUND(AVG(freeze_temperature)::numeric, 3) AS FreezeTemperature,
                        ROUND(AVG(distance_to_surface)::numeric, 3) AS DistanceToSurface
                    FROM public.vw_dspd_data_full
                    WHERE sensor_id = @sensorId
                        AND received_at >= @fromDate
                    GROUP BY DATE_TRUNC('hour', received_at)
                    ORDER BY ReceivedAt ASC";
                
                var measurements = new List<DSPDMeasurementViewModel>();
                
                using (var connection = new NpgsqlConnection(connectionString))
                {
                    using (var command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@sensorId", sensorId);
                        command.Parameters.AddWithValue("@fromDate", fromDate);
                        
                        await connection.OpenAsync();
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var measurement = new DSPDMeasurementViewModel
                                {
                                    ReceivedAt = reader.GetDateTime(reader.GetOrdinal("ReceivedAt")),
                                    GripCoefficient = GetDecimalOrNull(reader, "GripCoefficient"),
                                    ShakeLevel = GetDecimalOrNull(reader, "ShakeLevel"),
                                    VoltagePower = GetDecimalOrNull(reader, "VoltagePower"),
                                    CaseTemperature = GetDecimalOrNull(reader, "CaseTemperature"),
                                    RoadTemperature = GetDecimalOrNull(reader, "RoadTemperature"),
                                    WaterHeight = GetDecimalOrNull(reader, "WaterHeight"),
                                    IceHeight = GetDecimalOrNull(reader, "IceHeight"),
                                    SnowHeight = GetDecimalOrNull(reader, "SnowHeight"),
                                    IcePercentage = GetDecimalOrNull(reader, "IcePercentage"),
                                    PgmPercentage = GetDecimalOrNull(reader, "PgmPercentage"),
                                    RoadStatusCode = GetInt32OrNull(reader, "RoadStatusCode"),
                                    RoadAngle = GetDecimalOrNull(reader, "RoadAngle"),
                                    FreezeTemperature = GetDecimalOrNull(reader, "FreezeTemperature"),
                                    DistanceToSurface = GetDecimalOrNull(reader, "DistanceToSurface")
                                };
                                
                                measurements.Add(measurement);
                            }
                        }
                    }
                }
                
                var sensor = await _context.Sensors
                    .Include(s => s.SensorType)
                    .Include(s => s.MonitoringPost)
                    .FirstOrDefaultAsync(s => s.Id == sensorId);
                
                var viewModel = new DSPDDataViewModel
                {
                    SensorId = sensorId,
                    SerialNumber = sensor?.SerialNumber ?? "",
                    EndpointName = sensor?.EndPointsName ?? "",
                    PostName = sensor?.MonitoringPost?.Name,
                    Measurements = measurements
                };
                
                return Json(viewModel);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при загрузке почасовых данных DSPD: {ex.Message}");
                return StatusCode(500, new { error = "Ошибка при загрузке почасовых данных" });
            }
        }


        [HttpGet]
        public async Task<IActionResult> GetDUSTDataHour(int sensorId, int days = 1)
        {
            try
            {
                var fromDate = DateTime.UtcNow.AddDays(-days);
                
                var connectionString = _context.Database.GetConnectionString();
                
                var query = @"
                    SELECT 
                        DATE_TRUNC('hour', received_at) AS ReceivedAt,
                        ROUND(AVG(pm10act)::numeric, 5) AS Pm10Act,
                        ROUND(AVG(pm25act)::numeric, 5) AS Pm25Act,
                        ROUND(AVG(pm1act)::numeric, 5) AS Pm1Act,
                        ROUND(AVG(pm10awg)::numeric, 5) AS Pm10Awg,
                        ROUND(AVG(pm25awg)::numeric, 5) AS Pm25Awg,
                        ROUND(AVG(pm1awg)::numeric, 5) AS Pm1Awg,
                        ROUND(AVG(flowprobe)::numeric, 5) AS FlowProbe,
                        ROUND(AVG(temperatureprobe)::numeric, 1) AS TemperatureProbe
                    FROM public.vw_dust_data_full
                    WHERE sensor_id = @sensorId
                        AND received_at >= @fromDate
                    GROUP BY DATE_TRUNC('hour', received_at)
                    ORDER BY ReceivedAt ASC";
                
                var measurements = new List<DUSTMeasurementViewModel>();
                
                using (var connection = new NpgsqlConnection(connectionString))
                {
                    using (var command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@sensorId", sensorId);
                        command.Parameters.AddWithValue("@fromDate", fromDate);
                        
                        await connection.OpenAsync();
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var measurement = new DUSTMeasurementViewModel
                                {
                                    ReceivedAt = reader.GetDateTime(reader.GetOrdinal("ReceivedAt")),
                                    Pm10Act = GetDecimalOrNull(reader, "Pm10Act"),
                                    Pm25Act = GetDecimalOrNull(reader, "Pm25Act"),
                                    Pm1Act = GetDecimalOrNull(reader, "Pm1Act"),
                                    Pm10Awg = GetDecimalOrNull(reader, "Pm10Awg"),
                                    Pm25Awg = GetDecimalOrNull(reader, "Pm25Awg"),
                                    Pm1Awg = GetDecimalOrNull(reader, "Pm1Awg"),
                                    FlowProbe = GetDecimalOrNull(reader, "FlowProbe"),
                                    TemperatureProbe = GetDecimalOrNull(reader, "TemperatureProbe"),
                                };
                                
                                measurements.Add(measurement);
                            }
                        }
                    }
                }
                
                var sensor = await _context.Sensors
                    .Include(s => s.SensorType)
                    .Include(s => s.MonitoringPost)
                    .FirstOrDefaultAsync(s => s.Id == sensorId);
                
                var viewModel = new DUSTDataViewModel
                {
                    SensorId = sensorId,
                    SerialNumber = sensor?.SerialNumber ?? "",
                    EndpointName = sensor?.EndPointsName ?? "",
                    PostName = sensor?.MonitoringPost?.Name,
                    Measurements = measurements
                };
                
                return Json(viewModel);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при загрузке почасовых данных DUST: {ex.Message}");
                return StatusCode(500, new { error = "Ошибка при загрузке почасовых данных" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetIWSDataHour(int sensorId, int days = 1)
        {
            try
            {
                var fromDate = DateTime.UtcNow.AddDays(-days);
                
                var connectionString = _context.Database.GetConnectionString();
                
                var query = @"
                    SELECT 
                        DATE_TRUNC('hour', received_at) AS ReceivedAt,
                        ROUND(AVG(environment_temperature)::numeric, 3) AS EnvironmentTemperature,
                        ROUND(AVG(humidity_percentage)::numeric, 3) AS HumidityPercentage,
                        ROUND(AVG(dew_point)::numeric, 3) AS DewPoint,
                        ROUND(AVG(pressure_hpa)::numeric, 3) AS PressureHpa,
                        ROUND(AVG(pressure_qnh_hpa)::numeric, 3) AS PressureQNHHpa,
                        ROUND(AVG(pressure_mmhg)::numeric, 3) AS PressureMmHg,
                        ROUND(AVG(wind_speed)::numeric, 3) AS WindSpeed,
                        ROUND(AVG(wind_direction)::numeric, 3) AS WindDirection,
                        ROUND(AVG(wind_vs_sound)::numeric, 3) AS WindVSound,
                        AVG(precipitation_type) AS PrecipitationType,
                        ROUND(AVG(precipitation_intensity)::numeric, 3) AS PrecipitationIntensity,
                        ROUND(AVG(precipitation_quantity)::numeric, 3) AS PrecipitationQuantity,
                        ROUND(AVG(precipitation_elapsed)::numeric, 3) AS PrecipitationElapsed,
                        ROUND(AVG(precipitation_period)::numeric, 3) AS PrecipitationPeriod,
                        ROUND(AVG(co2_level)::numeric, 3) AS Co2Level,
                        ROUND(AVG(supply_voltage)::numeric, 3) AS SupplyVoltage,
                        ROUND(AVG(ksp_value)::numeric, 3) AS KspValue,
                        ROUND(AVG(acceleration_std_dev)::numeric, 3) AS AccelerationStdDev
                    FROM public.vw_iws_data_full
                    WHERE sensor_id = @sensorId
                        AND received_at >= @fromDate
                    GROUP BY DATE_TRUNC('hour', received_at)
                    ORDER BY ReceivedAt ASC";
                
                var measurements = new List<IWSMeasurementViewModel>();
                
                using (var connection = new NpgsqlConnection(connectionString))
                {
                    using (var command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@sensorId", sensorId);
                        command.Parameters.AddWithValue("@fromDate", fromDate);
                        
                        await connection.OpenAsync();
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var measurement = new IWSMeasurementViewModel
                                {
                                    ReceivedAt = reader.GetDateTime(reader.GetOrdinal("ReceivedAt")),
                                    
                                    EnvironmentTemperature = GetDecimalOrNull(reader, "EnvironmentTemperature"),
                                    HumidityPercentage = GetDecimalOrNull(reader, "HumidityPercentage"),
                                    DewPoint = GetDecimalOrNull(reader, "DewPoint"),
                                    
                                    PressureHpa = GetDecimalOrNull(reader, "PressureHpa"),
                                    PressureQNHHpa = GetDecimalOrNull(reader, "PressureQNHHpa"),
                                    PressureMmHg = GetDecimalOrNull(reader, "PressureMmHg"),
                                    
                                    WindSpeed = GetDecimalOrNull(reader, "WindSpeed"),
                                    WindDirection = GetDecimalOrNull(reader, "WindDirection"),
                                    WindVSound = GetDecimalOrNull(reader, "WindVSound"),
                                    
                                    PrecipitationType = GetInt32OrNull(reader, "PrecipitationType"),
                                    PrecipitationIntensity = GetDecimalOrNull(reader, "PrecipitationIntensity"),
                                    PrecipitationQuantity = GetDecimalOrNull(reader, "PrecipitationQuantity"),
                                    PrecipitationElapsed = GetInt32OrNull(reader, "PrecipitationElapsed"),
                                    PrecipitationPeriod = GetInt32OrNull(reader, "PrecipitationPeriod"),
                                    
                                    Co2Level = GetDecimalOrNull(reader, "Co2Level"),
                                    SupplyVoltage = GetDecimalOrNull(reader, "SupplyVoltage"),
                                    
                                    KspValue = GetInt32OrNull(reader, "KspValue"),
                                    AccelerationStdDev = GetDecimalOrNull(reader, "AccelerationStdDev")
                                };
                                
                                measurements.Add(measurement);
                            }
                        }
                    }
                }
                
                var sensor = await _context.Sensors
                    .Include(s => s.SensorType)
                    .Include(s => s.MonitoringPost)
                    .FirstOrDefaultAsync(s => s.Id == sensorId);
                
                var viewModel = new IWSDataViewModel
                {
                    SensorId = sensorId,
                    SerialNumber = sensor?.SerialNumber ?? "",
                    EndpointName = sensor?.EndPointsName ?? "",
                    PostName = sensor?.MonitoringPost?.Name,
                    Measurements = measurements
                };
                
                return Json(viewModel);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при загрузке почасовых данных IWS: {ex.Message}");
                return StatusCode(500, new { error = "Ошибка при загрузке почасовых данных" });
            }
        }
       
        [HttpGet]
        public async Task<IActionResult> GetMUEKSDataHour(int sensorId, int days = 1)
        {
            try
            {
                var fromDate = DateTime.UtcNow.AddDays(-days);
                
                var connectionString = _context.Database.GetConnectionString();
                
                var query = @"
                    SELECT 
                        DATE_TRUNC('hour', received_at) AS ReceivedAt,
                        ROUND(AVG(temperature_box), 3) AS TemperatureBox,
                        ROUND(AVG(voltage_power_in_12b), 3) AS VoltagePowerIn12b,
                        ROUND(AVG(voltage_out_12b), 3) AS VoltageOut12b,
                        ROUND(AVG(current_out_12b), 3) AS CurrentOut12b,
                        ROUND(AVG(current_out_48b), 3) AS CurrentOut48b,
                        ROUND(AVG(voltage_akb), 3) AS VoltageAkb,
                        ROUND(AVG(current_akb), 3) AS CurrentAkb,
                        ROUND(AVG(sensor_220b), 3) AS Sensor220b,
                        ROUND(AVG(watt_hours_akb), 3) AS WattHoursAkb,
                        ROUND(AVG(visible_range), 3) AS VisibleRange,
                        -- Текстовые поля с числовыми значениями обрабатываем с проверкой
                        ROUND(AVG(NULLIF(NULLIF(tds_h, ''), 'NULL')::numeric), 3) AS TdsH,
                        ROUND(AVG(NULLIF(NULLIF(tds_tds, ''), 'NULL')::numeric), 3) AS TdsTds,
                        ROUND(AVG(NULLIF(NULLIF(tkosa_t1, ''), 'NULL')::numeric), 3) AS TkosaT1,
                        ROUND(AVG(NULLIF(NULLIF(tkosa_t3, ''), 'NULL')::numeric), 3) AS TkosaT3
                    FROM public.vw_mueks_data_full
                    WHERE sensor_id = @sensorId
                        AND received_at >= @fromDate
                    GROUP BY DATE_TRUNC('hour', received_at)
                    ORDER BY ReceivedAt ASC";
                
                var measurements = new List<MUEKSMeasurementViewModel>();
                
                using (var connection = new NpgsqlConnection(connectionString))
                {
                    using (var command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@sensorId", sensorId);
                        command.Parameters.AddWithValue("@fromDate", fromDate);
                        
                        await connection.OpenAsync();
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var measurement = new MUEKSMeasurementViewModel
                                {
                                    ReceivedAt = reader.GetDateTime(reader.GetOrdinal("ReceivedAt")),
                                    
                                    TemperatureBox = GetDecimalOrNull(reader, "TemperatureBox"),
                                    VoltagePowerIn12b = GetDecimalOrNull(reader, "VoltagePowerIn12b"),
                                    VoltageOut12b = GetDecimalOrNull(reader, "VoltageOut12b"),
                                    CurrentOut12b = GetDecimalOrNull(reader, "CurrentOut12b"),
                                    CurrentOut48b = GetDecimalOrNull(reader, "CurrentOut48b"),
                                    VoltageAkb = GetDecimalOrNull(reader, "VoltageAkb"),
                                    CurrentAkb = GetDecimalOrNull(reader, "CurrentAkb"),
                                    Sensor220b = GetInt32OrNull(reader, "Sensor220b"),
                                    WattHoursAkb = GetDecimalOrNull(reader, "WattHoursAkb"),
                                    VisibleRange = GetDecimalOrNull(reader, "VisibleRange"),
                                    
                                    // Текстовые поля с числовыми значениями
                                    TdsH = GetStringOrNull(reader, "TdsH"),
                                    TdsTds = GetStringOrNull(reader, "TdsTds"),
                                    TkosaT1 = GetStringOrNull(reader, "TkosaT1"),
                                    TkosaT3 = GetStringOrNull(reader, "TkosaT3")
                                };
                                
                                measurements.Add(measurement);
                            }
                        }
                    }
                }
                
                var sensor = await _context.Sensors
                    .Include(s => s.SensorType)
                    .Include(s => s.MonitoringPost)
                    .FirstOrDefaultAsync(s => s.Id == sensorId);
                
                var viewModel = new MUEKSDataViewModel
                {
                    SensorId = sensorId,
                    SerialNumber = sensor?.SerialNumber ?? "",
                    EndpointName = sensor?.EndPointsName ?? "",
                    PostName = sensor?.MonitoringPost?.Name,
                    Measurements = measurements
                };
                
                return Json(viewModel);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при загрузке почасовых данных MUEKS: {ex.Message}");
                return StatusCode(500, new { error = "Ошибка при загрузке почасовых данных" });
            }
        }

#endregion


#region Данные сгруппированные по 10 минут 

        [HttpGet]
        public async Task<IActionResult> GetDOVDataTenMinuteInterval(int sensorId, int days = 1)
        {
            try
            {
                var fromDate = DateTime.UtcNow.AddDays(-days);
               
                var connectionString = _context.Database.GetConnectionString();
               
                var query = @"
                    SELECT
                        received_at AS ReceivedAt,
                        visible_range AS VisibleRange
                    FROM public.vw_dov_data_full
                    WHERE sensor_id = @sensorId
                        AND received_at >= @fromDate
                    ORDER BY received_at ASC";
               
                var measurements = new List<DOVMeasurementViewModel>();
               
                using (var connection = new NpgsqlConnection(connectionString))
                {
                    using (var command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@sensorId", sensorId);
                        command.Parameters.AddWithValue("@fromDate", fromDate);
                       
                        await connection.OpenAsync();
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                measurements.Add(new DOVMeasurementViewModel
                                {
                                    ReceivedAt = reader.GetDateTime(reader.GetOrdinal("ReceivedAt")),
                                    VisibleRange = reader.GetDecimal(reader.GetOrdinal("VisibleRange"))
                                });
                            }
                        }
                    }
                }
                var sensor = await _context.Sensors
                    .Include(s => s.SensorType)
                    .Include(s => s.MonitoringPost)
                    .FirstOrDefaultAsync(s => s.Id == sensorId);
                var viewModel = new DOVDataViewModel
                {
                    SensorId = sensorId,
                    SerialNumber = sensor?.SerialNumber ?? "",
                    EndpointName = sensor?.EndPointsName ?? "",
                    PostName = sensor?.MonitoringPost?.Name,
                    Measurements = measurements
                };
                Console.WriteLine(Json(viewModel).ToString());
                return Json(viewModel);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при загрузке данных DOV: {ex.Message}");
                return StatusCode(500, new { error = "Ошибка при загрузке данных" });
            }
        }
       
        [HttpGet]
        public async Task<IActionResult> GetDSPDDataTenMinuteInterval(int sensorId, int days = 1)
        {
            try
            {
                var fromDate = DateTime.UtcNow.AddDays(-days);
               
                var connectionString = _context.Database.GetConnectionString();
               
                var query = @"
                    SELECT
                        received_at AS ReceivedAt,
                        grip_coefficient AS GripCoefficient,
                        shake_level AS ShakeLevel,
                        voltage_power AS VoltagePower,
                        case_temperature AS CaseTemperature,
                        road_temperature AS RoadTemperature,
                        water_height AS WaterHeight,
                        ice_height AS IceHeight,
                        snow_height AS SnowHeight,
                        ice_percentage AS IcePercentage,
                        pgm_percentage AS PgmPercentage,
                        road_status_code AS RoadStatusCode,
                        road_angle AS RoadAngle,
                        freeze_temperature AS FreezeTemperature,
                        distance_to_surface AS DistanceToSurface
                    FROM public.vw_dspd_data_full
                    WHERE sensor_id = @sensorId
                        AND received_at >= @fromDate
                    ORDER BY received_at ASC";
               
                var measurements = new List<DSPDMeasurementViewModel>();
               
                using (var connection = new NpgsqlConnection(connectionString))
                {
                    using (var command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@sensorId", sensorId);
                        command.Parameters.AddWithValue("@fromDate", fromDate);
                       
                        await connection.OpenAsync();
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var measurement = new DSPDMeasurementViewModel
                                {
                                    ReceivedAt = reader.GetDateTime(reader.GetOrdinal("ReceivedAt")),
                                    GripCoefficient = GetDecimalOrNull(reader, "GripCoefficient"),
                                    ShakeLevel = GetDecimalOrNull(reader, "ShakeLevel"),
                                    VoltagePower = GetDecimalOrNull(reader, "VoltagePower"),
                                    CaseTemperature = GetDecimalOrNull(reader, "CaseTemperature"),
                                    RoadTemperature = GetDecimalOrNull(reader, "RoadTemperature"),
                                    WaterHeight = GetDecimalOrNull(reader, "WaterHeight"),
                                    IceHeight = GetDecimalOrNull(reader, "IceHeight"),
                                    SnowHeight = GetDecimalOrNull(reader, "SnowHeight"),
                                    IcePercentage = GetDecimalOrNull(reader, "IcePercentage"),
                                    PgmPercentage = GetDecimalOrNull(reader, "PgmPercentage"),
                                    RoadStatusCode = GetInt32OrNull(reader, "RoadStatusCode"),
                                    RoadAngle = GetDecimalOrNull(reader, "RoadAngle"),
                                    FreezeTemperature = GetDecimalOrNull(reader, "FreezeTemperature"),
                                    DistanceToSurface = GetDecimalOrNull(reader, "DistanceToSurface")
                                };
                               
                                measurements.Add(measurement);
                            }
                        }
                    }
                }
                var sensor = await _context.Sensors
                    .Include(s => s.SensorType)
                    .Include(s => s.MonitoringPost)
                    .FirstOrDefaultAsync(s => s.Id == sensorId);
                var viewModel = new DSPDDataViewModel
                {
                    SensorId = sensorId,
                    SerialNumber = sensor?.SerialNumber ?? "",
                    EndpointName = sensor?.EndPointsName ?? "",
                    PostName = sensor?.MonitoringPost?.Name,
                    Measurements = measurements
                };
                return Json(viewModel);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при загрузке данных DSPD: {ex.Message}");
                return StatusCode(500, new { error = "Ошибка при загрузке данных" });
            }
        }
        
        [HttpGet]
        public async Task<IActionResult> GetDUSTDataTenMinuteInterval(int sensorId, int days = 1)
        {
            try
            {
                var fromDate = DateTime.UtcNow.AddDays(-days);
               
                var connectionString = _context.Database.GetConnectionString();
               
                var query = @"
                    SELECT
                        received_at AS ReceivedAt,
                        pm10act AS Pm10Act,
                        pm25act AS Pm25Act,
                        pm1act AS Pm1Act,
                        pm10awg AS Pm10Awg,
                        pm25awg AS Pm25Awg,
                        pm1awg AS Pm1Awg,
                        flowprobe AS FlowProbe,
                        temperatureprobe AS TemperatureProbe,
                        humidityprobe AS HumidityProbe,
                        laserstatus AS LaserStatus,
                        supplyvoltage AS SupplyVoltage
                    FROM public.vw_dust_data_full
                    WHERE sensor_id = @sensorId
                        AND received_at >= @fromDate
                    ORDER BY received_at ASC";
               
                var measurements = new List<DUSTMeasurementViewModel>();
               
                using (var connection = new NpgsqlConnection(connectionString))
                {
                    using (var command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@sensorId", sensorId);
                        command.Parameters.AddWithValue("@fromDate", fromDate);
                       
                        await connection.OpenAsync();
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var measurement = new DUSTMeasurementViewModel
                                {
                                    ReceivedAt = reader.GetDateTime(reader.GetOrdinal("ReceivedAt")),
                                    Pm10Act = GetDecimalOrNull(reader, "Pm10Act"),
                                    Pm25Act = GetDecimalOrNull(reader, "Pm25Act"),
                                    Pm1Act = GetDecimalOrNull(reader, "Pm1Act"),
                                    Pm10Awg = GetDecimalOrNull(reader, "Pm10Awg"),
                                    Pm25Awg = GetDecimalOrNull(reader, "Pm25Awg"),
                                    Pm1Awg = GetDecimalOrNull(reader, "Pm1Awg"),
                                    FlowProbe = GetDecimalOrNull(reader, "FlowProbe"),
                                    TemperatureProbe = GetDecimalOrNull(reader, "TemperatureProbe")
                                };
                               
                                measurements.Add(measurement);
                            }
                        }
                    }
                }
                var sensor = await _context.Sensors
                    .Include(s => s.SensorType)
                    .Include(s => s.MonitoringPost)
                    .FirstOrDefaultAsync(s => s.Id == sensorId);
                var viewModel = new DUSTDataViewModel
                {
                    SensorId = sensorId,
                    SerialNumber = sensor?.SerialNumber ?? "",
                    EndpointName = sensor?.EndPointsName ?? "",
                    PostName = sensor?.MonitoringPost?.Name,
                    Measurements = measurements
                };
                return Json(viewModel);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при загрузке данных DUST: {ex.Message}");
                return StatusCode(500, new { error = "Ошибка при загрузке данных" });
            }
        }
      
        [HttpGet]
        public async Task<IActionResult> GetIWSDataTenMinuteInterval(int sensorId, int days = 1)
        {
            try
            {
                var fromDate = DateTime.UtcNow.AddDays(-days);
               
                var connectionString = _context.Database.GetConnectionString();
               
                var query = @"
                    SELECT
                        received_at AS ReceivedAt,
                        environment_temperature AS EnvironmentTemperature,
                        humidity_percentage AS HumidityPercentage,
                        dew_point AS DewPoint,
                        pressure_hpa AS PressureHpa,
                        pressure_qnh_hpa AS PressureQNHHpa,
                        pressure_mmhg AS PressureMmHg,
                        wind_speed AS WindSpeed,
                        wind_direction AS WindDirection,
                        wind_vs_sound AS WindVSound,
                        precipitation_type AS PrecipitationType,
                        precipitation_intensity AS PrecipitationIntensity,
                        precipitation_quantity AS PrecipitationQuantity,
                        precipitation_elapsed AS PrecipitationElapsed,
                        precipitation_period AS PrecipitationPeriod,
                        co2_level AS Co2Level,
                        supply_voltage AS SupplyVoltage,
                        iws_latitude AS IwsLatitude,
                        iws_longitude AS IwsLongitude,
                        altitude AS Altitude,
                        ksp_value AS KspValue,
                        gps_speed AS GpsSpeed,
                        acceleration_std_dev AS AccelerationStdDev,
                        roll_angle AS RollAngle,
                        pitch_angle AS PitchAngle
                    FROM public.vw_iws_data_full
                    WHERE sensor_id = @sensorId
                        AND received_at >= @fromDate
                    ORDER BY received_at ASC";
               
                var measurements = new List<IWSMeasurementViewModel>();
               
                using (var connection = new NpgsqlConnection(connectionString))
                {
                    using (var command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@sensorId", sensorId);
                        command.Parameters.AddWithValue("@fromDate", fromDate);
                       
                        await connection.OpenAsync();
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var measurement = new IWSMeasurementViewModel
                                {
                                    ReceivedAt = reader.GetDateTime(reader.GetOrdinal("ReceivedAt")),
                                   
                                    EnvironmentTemperature = GetDecimalOrNull(reader, "EnvironmentTemperature"),
                                    HumidityPercentage = GetDecimalOrNull(reader, "HumidityPercentage"),
                                    DewPoint = GetDecimalOrNull(reader, "DewPoint"),
                                   
                                    PressureHpa = GetDecimalOrNull(reader, "PressureHpa"),
                                    PressureQNHHpa = GetDecimalOrNull(reader, "PressureQNHHpa"),
                                    PressureMmHg = GetDecimalOrNull(reader, "PressureMmHg"),
                                   
                                    WindSpeed = GetDecimalOrNull(reader, "WindSpeed"),
                                    WindDirection = GetDecimalOrNull(reader, "WindDirection"),
                                    WindVSound = GetDecimalOrNull(reader, "WindVSound"),
                                   
                                    PrecipitationType = GetInt32OrNull(reader, "PrecipitationType"),
                                    PrecipitationIntensity = GetDecimalOrNull(reader, "PrecipitationIntensity"),
                                    PrecipitationQuantity = GetDecimalOrNull(reader, "PrecipitationQuantity"),
                                    PrecipitationElapsed = GetInt32OrNull(reader, "PrecipitationElapsed"),
                                    PrecipitationPeriod = GetInt32OrNull(reader, "PrecipitationPeriod"),
                                   
                                    Co2Level = GetDecimalOrNull(reader, "Co2Level"),
                                    SupplyVoltage = GetDecimalOrNull(reader, "SupplyVoltage"),
                                   
                                    IwsLatitude = GetDecimalOrNull(reader, "IwsLatitude"),
                                    IwsLongitude = GetDecimalOrNull(reader, "IwsLongitude"),
                                    Altitude = GetDecimalOrNull(reader, "Altitude"),
                                   
                                    KspValue = GetInt32OrNull(reader, "KspValue"),
                                    GpsSpeed = GetDecimalOrNull(reader, "GpsSpeed"),
                                    AccelerationStdDev = GetDecimalOrNull(reader, "AccelerationStdDev"),
                                    RollAngle = GetDecimalOrNull(reader, "RollAngle"),
                                    PitchAngle = GetDecimalOrNull(reader, "PitchAngle")
                                };
                               
                                measurements.Add(measurement);
                            }
                        }
                    }
                }
                var sensor = await _context.Sensors
                    .Include(s => s.SensorType)
                    .Include(s => s.MonitoringPost)
                    .FirstOrDefaultAsync(s => s.Id == sensorId);
                var viewModel = new IWSDataViewModel
                {
                    SensorId = sensorId,
                    SerialNumber = sensor?.SerialNumber ?? "",
                    EndpointName = sensor?.EndPointsName ?? "",
                    PostName = sensor?.MonitoringPost?.Name,
                    Measurements = measurements
                };
                return Json(viewModel);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при загрузке данных IWS: {ex.Message}");
                return StatusCode(500, new { error = "Ошибка при загрузке данных" });
            }
        }
       
        [HttpGet]
        public async Task<IActionResult> GetMUEKSDataTenMinuteInterval(int sensorId, int days = 1)
        {
            try
            {
                var fromDate = DateTime.UtcNow.AddDays(-days);
               
                var connectionString = _context.Database.GetConnectionString();
               
                var query = @"
                    SELECT
                        mueks_data_id AS MueksDataId,
                        received_at AS ReceivedAt,
                        temperature_box AS TemperatureBox,
                        voltage_power_in_12b AS VoltagePowerIn12b,
                        voltage_out_12b AS VoltageOut12b,
                        current_out_12b AS CurrentOut12b,
                        current_out_48b AS CurrentOut48b,
                        voltage_akb AS VoltageAkb,
                        current_akb AS CurrentAkb,
                        sensor_220b AS Sensor220b,
                        watt_hours_akb AS WattHoursAkb,
                        visible_range AS VisibleRange,
                        door_status AS DoorStatus,
                        tds_h AS TdsH,
                        tds_tds AS TdsTds,
                        tkosa_t1 AS TkosaT1,
                        tkosa_t3 AS TkosaT3
                    FROM public.vw_mueks_data_full
                    WHERE sensor_id = @sensorId
                        AND received_at >= @fromDate
                    ORDER BY received_at ASC";
               
                var measurements = new List<MUEKSMeasurementViewModel>();
               
                using (var connection = new NpgsqlConnection(connectionString))
                {
                    using (var command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@sensorId", sensorId);
                        command.Parameters.AddWithValue("@fromDate", fromDate);
                       
                        await connection.OpenAsync();
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var measurement = new MUEKSMeasurementViewModel
                                {
                                    ReceivedAt = reader.GetDateTime(reader.GetOrdinal("ReceivedAt")),
                                   
                                    TemperatureBox = GetDecimalOrNull(reader, "TemperatureBox"),
                                    VoltagePowerIn12b = GetDecimalOrNull(reader, "VoltagePowerIn12b"),
                                    VoltageOut12b = GetDecimalOrNull(reader, "VoltageOut12b"),
                                    CurrentOut12b = GetDecimalOrNull(reader, "CurrentOut12b"),
                                    CurrentOut48b = GetDecimalOrNull(reader, "CurrentOut48b"),
                                    VoltageAkb = GetDecimalOrNull(reader, "VoltageAkb"),
                                    CurrentAkb = GetDecimalOrNull(reader, "CurrentAkb"),
                                    Sensor220b = GetInt32OrNull(reader, "Sensor220b"),
                                    WattHoursAkb = GetDecimalOrNull(reader, "WattHoursAkb"),
                                    VisibleRange = GetDecimalOrNull(reader, "VisibleRange"),
                                    DoorStatus = GetInt32OrNull(reader, "DoorStatus"),
                                   
                                    TdsH = GetStringOrNull(reader, "TdsH"),
                                    TdsTds = GetStringOrNull(reader, "TdsTds"),
                                    TkosaT1 = GetStringOrNull(reader, "TkosaT1"),
                                    TkosaT3 = GetStringOrNull(reader, "TkosaT3")
                                };
                               
                                measurements.Add(measurement);
                            }
                        }
                    }
                }
                var sensor = await _context.Sensors
                    .Include(s => s.SensorType)
                    .Include(s => s.MonitoringPost)
                    .FirstOrDefaultAsync(s => s.Id == sensorId);
                var viewModel = new MUEKSDataViewModel
                {
                    SensorId = sensorId,
                    SerialNumber = sensor?.SerialNumber ?? "",
                    EndpointName = sensor?.EndPointsName ?? "",
                    PostName = sensor?.MonitoringPost?.Name,
                    Measurements = measurements
                };
                return Json(viewModel);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при загрузке данных MUEKS: {ex.Message}");
                return StatusCode(500, new { error = "Ошибка при загрузке данных" });
            }
        }

#endregion
        

        // Добавьте вспомогательный метод для чтения строк
        private string GetStringOrNull(NpgsqlDataReader reader, string columnName)
        {
            var ordinal = reader.GetOrdinal(columnName);
            return reader.IsDBNull(ordinal) ? null : reader.GetString(ordinal);
        }
        // Вспомогательные методы для безопасного чтения NULL значений
        private decimal? GetDecimalOrNull(NpgsqlDataReader reader, string columnName)
        {
            var ordinal = reader.GetOrdinal(columnName);
            return reader.IsDBNull(ordinal) ? (decimal?)null : reader.GetDecimal(ordinal);
        }
        private int? GetInt32OrNull(NpgsqlDataReader reader, string columnName)
        {
            var ordinal = reader.GetOrdinal(columnName);
            return reader.IsDBNull(ordinal) ? (int?)null : reader.GetInt32(ordinal);
        }
        private bool? GetBooleanOrNull(NpgsqlDataReader reader, string columnName)
        {
            var ordinal = reader.GetOrdinal(columnName);
            return reader.IsDBNull(ordinal) ? (bool?)null : reader.GetBoolean(ordinal);
        }
    }
}