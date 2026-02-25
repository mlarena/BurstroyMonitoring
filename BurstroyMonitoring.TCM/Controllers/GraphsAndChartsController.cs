using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BurstroyMonitoring.Data;
using BurstroyMonitoring.Data.Models;
using BurstroyMonitoring.Data.Models.ViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BurstroyMonitoring.TCM.Controllers
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
            ViewData["Title"] = "Графики и диаграммы";
            
            var model = new SensorSelectionViewModel();
            
            // Загружаем посты мониторинга
            model.MonitoringPosts = await _context.MonitoringPosts
                .Where(p => p.IsActive)
                .OrderBy(p => p.Name)
                .Select(p => new SelectListItem
                {
                    Value = p.Id.ToString(),
                    Text = p.Name
                })
                .ToListAsync();
            
            model.MonitoringPosts.Insert(0, new SelectListItem { Value = "", Text = "Все посты" });
            
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> GetSensorsByPost(int monitoringPostId)
        {
            var query = _context.Sensors
                .Include(s => s.SensorType)
                .Where(s => s.IsActive);

            if (monitoringPostId > 0)
            {
                query = query.Where(s => s.MonitoringPostId == monitoringPostId);
            }

            var sensors = await query
                .OrderBy(s => s.SensorType!.SensorTypeName)
                .ThenBy(s => s.EndPointsName)
                .Select(s => new SelectListItem
                {
                    Value = s.Id.ToString(),
                    Text = $"{s.SensorType!.SensorTypeName} - {s.EndPointsName} ({s.SerialNumber})"
                })
                .ToListAsync();

            return Json(sensors);
        }

        [HttpGet]
        public async Task<IActionResult> GetSensorData(int sensorId)
        {
            var sensor = await _context.Sensors
                .Include(s => s.SensorType)
                .Include(s => s.MonitoringPost)
                .FirstOrDefaultAsync(s => s.Id == sensorId);

            if (sensor == null)
            {
                return NotFound();
            }

            var viewModel = new SensorViewModel
            {
                Id = sensor.Id,
                SensorTypeName = sensor.SensorType?.SensorTypeName ?? "Unknown",
                EndPointsName = sensor.EndPointsName,
                SerialNumber = sensor.SerialNumber,
                MonitoringPostName = sensor.MonitoringPost?.Name
            };

            // Выбираем соответствующий партиал в зависимости от типа датчика
            string partialName = sensor.SensorType?.SensorTypeName?.ToUpper() switch
            {
                "DSPD" => "_DSPDPartial",
                "IWS" => "_IWSPartial",
                "DOV" => "_DOVPartial",
                "DUST" => "_DUSTPartial",
                "MUEKS" => "_MUEKSPartial",
                _ => "_DefaultPartial"
            };

            return PartialView(partialName, viewModel);
        }

        #region API Methods for Charts

        [HttpGet]
        public async Task<IActionResult> GetDOVData(int sensorId, DateTime? startDate, DateTime? endDate)
        {
            var end = endDate ?? DateTime.UtcNow;
            var start = startDate ?? end.AddDays(-1);

            var data = await _context.VwDovDataFull
                .Where(d => d.SensorId == sensorId && 
                       d.DataTimestamp >= start && 
                       d.DataTimestamp <= end)
                .OrderBy(d => d.DataTimestamp)
                .Select(d => new DOVMeasurementViewModel
                {
                    DovDataId = d.DovDataId,
                    DataTimestamp = d.DataTimestamp ?? DateTime.MinValue,
                    VisibleRange = d.VisibleRange ?? 0,
                    BrightFlag = d.BrightFlag ?? 0
                })
                .ToListAsync();

            return Json(data);
        }

        [HttpGet]
        public async Task<IActionResult> GetDSPDData(int sensorId, DateTime? startDate, DateTime? endDate)
        {
            var end = endDate ?? DateTime.UtcNow;
            var start = startDate ?? end.AddDays(-1);

            var data = await _context.VwDspdDataFull
                .Where(d => d.SensorId == sensorId && 
                       d.DataTimestamp >= start && 
                       d.DataTimestamp <= end)
                .OrderBy(d => d.DataTimestamp)
                .Select(d => new DSPDMeasurementViewModel
                {
                    DspdDataId = d.DspdDataId,
                    DataTimestamp = d.DataTimestamp ?? DateTime.MinValue,
                    GripCoefficient = d.GripCoefficient,
                    ShakeLevel = d.ShakeLevel,
                    VoltagePower = d.VoltagePower,
                    CaseTemperature = d.CaseTemperature,
                    RoadTemperature = d.RoadTemperature,
                    WaterHeight = d.WaterHeight,
                    IceHeight = d.IceHeight,
                    SnowHeight = d.SnowHeight,
                    IcePercentage = d.IcePercentage,
                    PgmPercentage = d.PgmPercentage,
                    RoadStatusCode = d.RoadStatusCode,
                    RoadAngle = d.RoadAngle,
                    FreezeTemperature = d.FreezeTemperature,
                    DistanceToSurface = (decimal?)d.DistanceToSurface,
                    CalibrationNeeded = d.CalibrationNeeded,
                    GpsLatitude = d.GpsLatitude,
                    GpsLongitude = d.GpsLongitude,
                    GpsValid = d.GpsValid
                })
                .ToListAsync();

            return Json(data);
        }

        [HttpGet]
        public async Task<IActionResult> GetDUSTData(int sensorId, DateTime? startDate, DateTime? endDate)
        {
            var end = endDate ?? DateTime.UtcNow;
            var start = startDate ?? end.AddDays(-1);

            var data = await _context.VwDustDataFull
                .Where(d => d.SensorId == sensorId && 
                       d.DataTimestamp >= start && 
                       d.DataTimestamp <= end)
                .OrderBy(d => d.DataTimestamp)
                .Select(d => new DUSTMeasurementViewModel
                {
                    DustDataId = d.DustDataId,
                    DataTimestamp = d.DataTimestamp ?? DateTime.MinValue,
                    Pm10Act = d.PM10Act,
                    Pm25Act = d.PM25Act,
                    Pm1Act = d.PM1Act,
                    Pm10Awg = d.PM10AWG,
                    Pm25Awg = d.PM25AWG,
                    Pm1Awg = d.PM1AWG,
                    FlowProbe = d.FlowProbe,
                    TemperatureProbe = d.TemperatureProbe,
                    HumidityProbe = d.HumidityProbe,
                    LaserStatus = d.LaserStatus,
                    SupplyVoltage = d.SupplyVoltage
                })
                .ToListAsync();

            return Json(data);
        }

        [HttpGet]
        public async Task<IActionResult> GetIWSData(int sensorId, DateTime? startDate, DateTime? endDate)
        {
            var end = endDate ?? DateTime.UtcNow;
            var start = startDate ?? end.AddDays(-1);

            var data = await _context.VwIwsDataFull
                .Where(d => d.SensorId == sensorId && 
                       d.DataTimestamp >= start && 
                       d.DataTimestamp <= end)
                .OrderBy(d => d.DataTimestamp)
                .Select(d => new IWSMeasurementViewModel
                {
                    IwsDataId = d.IwsDataId,
                    DataTimestamp = d.DataTimestamp ?? DateTime.MinValue,
                    EnvironmentTemperature = d.EnvironmentTemperature,
                    HumidityPercentage = d.HumidityPercentage,
                    DewPoint = d.DewPoint,
                    PressureHpa = d.PressureHpa,
                    PressureQNHHpa = d.PressureQnhHpa,
                    PressureMmHg = d.PressureMmhg,
                    WindSpeed = d.WindSpeed,
                    WindDirection = d.WindDirection,
                    WindVSound = d.WindVsSound,
                    PrecipitationType = d.PrecipitationType,
                    PrecipitationIntensity = d.PrecipitationIntensity,
                    PrecipitationQuantity = d.PrecipitationQuantity,
                    PrecipitationElapsed = d.PrecipitationElapsed,
                    PrecipitationPeriod = d.PrecipitationPeriod,
                    Co2Level = d.CO2Level,
                    SupplyVoltage = d.SupplyVoltage,
                    IwsLatitude = d.IwsLatitude,
                    IwsLongitude = d.IwsLongitude,
                    Altitude = d.Altitude,
                    GpsSpeed = d.GpsSpeed,
                    KspValue = d.KspValue,
                    AccelerationStdDev = d.AccelerationStdDev,
                    RollAngle = d.RollAngle,
                    PitchAngle = d.PitchAngle,
                    StatusOk = d.StatusOk
                })
                .ToListAsync();

            return Json(data);
        }

        [HttpGet]
        public async Task<IActionResult> GetMUEKSData(int sensorId, DateTime? startDate, DateTime? endDate)
        {
            var end = endDate ?? DateTime.UtcNow;
            var start = startDate ?? end.AddDays(-7);

            var data = await _context.VwMueksDataFull
                .Where(d => d.SensorId == sensorId && 
                       d.DataTimestamp >= start && 
                       d.DataTimestamp <= end)
                .OrderBy(d => d.DataTimestamp)
                .Select(d => new MUEKSMeasurementViewModel
                {
                    MueksDataId = d.MueksDataId,
                    DataTimestamp = d.DataTimestamp ?? DateTime.MinValue,
                    TemperatureBox = d.TemperatureBox,
                    VoltagePowerIn12b = d.VoltagePowerIn12B,
                    VoltageOut12b = d.VoltageOut12B,
                    VoltageAkb = d.VoltageAkb,
                    CurrentOut12b = d.CurrentOut12B,
                    CurrentOut48b = d.CurrentOut48B,
                    CurrentAkb = d.CurrentAkb,
                    WattHoursAkb = d.WattHoursAkb,
                    VisibleRange = d.VisibleRange,
                    Sensor220b = d.Sensor220B,
                    DoorStatus = d.DoorStatus,
                    TdsH = d.TdsH,
                    TdsTds = d.TdsTds,
                    TkosaT1 = d.TkosaT1,
                    TkosaT3 = d.TkosaT3
                })
                .ToListAsync();

            return Json(data);
        }

        #endregion
    }
}