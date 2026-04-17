using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BurstroyMonitoring.Data;
using BurstroyMonitoring.Data.Models;
using System.Linq;
using System.Threading.Tasks;

namespace BurstroyMonitoring.TCM.Controllers
{
    public class MonitoringMapController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<MonitoringMapController> _logger;

        public MonitoringMapController(ApplicationDbContext context, ILogger<MonitoringMapController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                // Убираем фильтр по активности - получаем все посты
                var posts = await _context.MonitoringPosts
                    .Include(p => p.Sensors) // Убираем фильтр Where(s => s.IsActive)
                    .ToListAsync();

                // Получаем все датчики без постов (и активные и неактивные)
                var sensorsWithoutPost = await _context.Sensors
                    .Where(s => s.MonitoringPostId == null && s.Latitude != null && s.Longitude != null)
                    .ToListAsync();

                ViewData["TotalPosts"] = posts.Count;
                ViewData["TotalSensors"] = posts.Sum(p => p.Sensors.Count) + sensorsWithoutPost.Count;
                ViewData["ActivePosts"] = posts.Count(p => p.IsActive);
                ViewData["InactivePosts"] = posts.Count(p => !p.IsActive);
                ViewData["ActiveSensors"] = posts.Sum(p => p.Sensors.Count(s => s.IsActive)) + sensorsWithoutPost.Count(s => s.IsActive);
                ViewData["InactiveSensors"] = posts.Sum(p => p.Sensors.Count(s => !s.IsActive)) + sensorsWithoutPost.Count(s => !s.IsActive);

                return View(posts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in MonitoringMapController.Index");
                return View(new List<MonitoringPost>());
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetMapData()
        {
            try
            {
                // Получаем все посты с координатами (и активные и неактивные)
                var posts = await _context.MonitoringPosts
                    .Where(p => p.Latitude != null && p.Longitude != null) // Убираем фильтр p.IsActive
                    .Select(p => new
                    {
                        Id = p.Id,
                        Type = "post",
                        Name = p.Name,
                        Address = p.Address,
                        Description = p.Description,
                        Latitude = p.Latitude,
                        Longitude = p.Longitude,
                        IsMobile = p.IsMobile,
                        IsActive = p.IsActive,
                        SensorCount = p.Sensors.Count, // Считаем все датчики
                        Sensors = p.Sensors.Select(s => new {
                            s.Id,
                            s.EndPointsName,
                            SensorTypeName = s.SensorType != null ? s.SensorType.SensorTypeName : "",
                            s.SensorTypeId
                        }).ToList()
                    })
                    .ToListAsync();

                // Получаем все датчики с координатами (и активные и неактивные)
                var sensors = await _context.Sensors
                    .Where(s => s.Latitude != null && s.Longitude != null) // Убираем фильтр s.IsActive
                    .Select(s => new
                    {
                        Id = s.Id,
                        Type = "sensor",
                        Name = s.EndPointsName,
                        SerialNumber = s.SerialNumber,
                        Description = s.Url,
                        Latitude = s.Latitude,
                        Longitude = s.Longitude,
                        SensorTypeId = s.SensorTypeId,
                        LastActivityUTC = s.LastActivityUTC,
                        MonitoringPostId = s.MonitoringPostId,
                        IsActive = s.IsActive,
                        SensorTypeName = s.SensorType != null ? s.SensorType.SensorTypeName : ""
                    })
                    .ToListAsync();

                return Json(new { posts, sensors });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in MonitoringMapController.GetMapData");
                return Json(new { posts = new object[] { }, sensors = new object[] { } });
            }
        }

        [HttpGet]
        public async Task<IActionResult> SearchPosts(string query)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(query) || query.Length < 2)
                    return Json(new object[] { });

                // Поиск без учета регистра (и активные и неактивные)
                var posts = await _context.MonitoringPosts
                    .Where(p => EF.Functions.ILike(p.Name, $"%{query}%") && p.Latitude != null && p.Longitude != null)
                    .Select(p => new
                    {
                        p.Id,
                        p.Name,
                        p.Latitude,
                        p.Longitude,
                        p.IsActive
                    })
                    .Take(10)
                    .ToListAsync();

                return Json(posts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in MonitoringMapController.SearchPosts for query: {Query}", query);
                return Json(new object[] { });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetLatestSensorData(int sensorId, int sensorTypeId)
        {
            try
            {
                // Получаем информацию о датчике, его типе и посте
                var sensor = await _context.Sensors
                    .Include(s => s.SensorType)
                    .Include(s => s.MonitoringPost)
                    .FirstOrDefaultAsync(s => s.Id == sensorId);

                if (sensor == null)
                    return NotFound();

                string sensorTypeName = sensor.SensorType?.SensorTypeName ?? "Unknown";
                string postName = sensor.MonitoringPost?.Name ?? "Без поста";
                string postAddress = sensor.MonitoringPost?.Address ?? "Без адеса";
                
                string endPointsName = sensor.EndPointsName;
                
                _logger.LogDebug("=== ЗАПРОС ДАННЫХ ДАТЧИКА === SensorId: {SensorId}, Post: {Post}, Type: {Type}, Endpoint: {Endpoint}", 
                    sensorId, postName, sensorTypeName, endPointsName);

                dynamic latestData = null;

                // Маппинг на основе имени типа
                switch (sensorTypeName.ToUpper())
                {
                    case "DSPD":
                        var dspdDirect = await _context.DSPDData
                            .Where(d => d.SensorId == sensorId)
                            .OrderByDescending(d => d.ReceivedAt)
                            .FirstOrDefaultAsync();
                        
                        if (dspdDirect != null)
                        {
                            latestData = new
                            {
                                dspdDirect.Grip,
                                // dspdDirect.Shake,
                                // dspdDirect.UPower,
                                // dspdDirect.TemperatureCase,
                                dspdDirect.TemperatureRoad,
                                dspdDirect.HeightH2O,
                                dspdDirect.HeightIce,
                                dspdDirect.HeightSnow,
                                dspdDirect.PercentICE,
                                dspdDirect.PercentPGM,
                                dspdDirect.RoadStatus,
                                // dspdDirect.AngleToRoad,
                                dspdDirect.TemperatureFreezePGM,
                                // dspdDirect.NeedCalibration,
                                dspdDirect.GPSLatitude,
                                dspdDirect.GPSLongitude,
                                // IsGpsValid = dspdDirect.IsGpsValid,
                                dspdDirect.DistanceToSurface,
                                dspdDirect.ReceivedAt,
                                dspdDirect.DataTimestamp
                            };
                        }
                        break;
                        
                    case "IWS":
                        var iwsDirect = await _context.IWSData
                            .Where(d => d.SensorId == sensorId)
                            .OrderByDescending(d => d.ReceivedAt)
                            .FirstOrDefaultAsync();
                        
                        if (iwsDirect != null)
                        {
                            latestData = new
                            {
                                iwsDirect.EnvTemperature,
                                iwsDirect.Humidity,
                                iwsDirect.DewPoint,
                                iwsDirect.PressureHPa,
                                // iwsDirect.PressureQNHHPa,
                                iwsDirect.PressureMmHg,
                                iwsDirect.WindSpeed,
                                iwsDirect.WindDirection,
                                iwsDirect.WindVSound,
                                iwsDirect.PrecipitationType,
                                iwsDirect.PrecipitationIntensity,
                                iwsDirect.PrecipitationQuantity,
                                iwsDirect.PrecipitationElapsed,
                                iwsDirect.PrecipitationPeriod,
                                iwsDirect.CO2Level,
                                iwsDirect.SupplyVoltage,
                                // iwsDirect.Latitude,
                                // iwsDirect.Longitude,
                                iwsDirect.Altitude,
                                // iwsDirect.KSP,
                                // iwsDirect.GPSSpeed,
                                // iwsDirect.AccelerationStDev,
                                // iwsDirect.Roll,
                                // iwsDirect.Pitch,
                                // iwsDirect.WeAreFine,
                                iwsDirect.ReceivedAt,
                                iwsDirect.DataTimestamp
                            };
                        }
                        break;
                        
                    case "DOV":
                        var dovDirect = await _context.DOVData
                            .Where(d => d.SensorId == sensorId)
                            .OrderByDescending(d => d.ReceivedAt)
                            .FirstOrDefaultAsync();
                        
                        if (dovDirect != null)
                        {
                            latestData = new
                            {
                                dovDirect.VisibleRange,
                                // dovDirect.BrightFlag,
                                dovDirect.ReceivedAt,
                                dovDirect.DataTimestamp
                            };
                        }
                        break;
                        
                    case "DUST":
                        var dustDirect = await _context.DustData
                            .Where(d => d.SensorId == sensorId)
                            .OrderByDescending(d => d.ReceivedAt)
                            .FirstOrDefaultAsync();
                        
                        if (dustDirect != null)
                        {
                            latestData = new
                            {
                                dustDirect.PM10Act,
                                dustDirect.PM25Act,
                                dustDirect.PM1Act,
                                dustDirect.PM10AWG,
                                dustDirect.PM25AWG,
                                dustDirect.PM1AWG,
                                dustDirect.FlowProbe,
                                dustDirect.TemperatureProbe,
                                dustDirect.HumidityProbe,
                                // dustDirect.LaserStatus,
                                // dustDirect.SupplyVoltage,
                                dustDirect.ReceivedAt,
                                dustDirect.DataTimestamp
                            };
                        }
                        break;
                        
                    case "MUEKS":
                        var mueksDirect = await _context.MUEKSData
                            .Where(d => d.SensorId == sensorId)
                            .OrderByDescending(d => d.ReceivedAt)
                            .FirstOrDefaultAsync();
                        
                        if (mueksDirect != null)
                        {
                            latestData = new
                            {
                                mueksDirect.TemperatureBox,
                                mueksDirect.UPowerIn12B,
                                mueksDirect.UOut12B,
                                mueksDirect.IOut12B,
                                mueksDirect.IOut48B,
                                mueksDirect.UAkb,
                                mueksDirect.IAkb,
                                mueksDirect.Sens220B,
                                mueksDirect.WhAkb,
                                mueksDirect.VisibleRange,
                                mueksDirect.DoorStatus,
                                mueksDirect.TdsH,
                                mueksDirect.TdsTds,
                                mueksDirect.TkosaT1,
                                mueksDirect.TkosaT3,
                                mueksDirect.ReceivedAt,
                                mueksDirect.DataTimestamp
                            };
                        }
                        break;
                }
                
                // Отладочный вывод
                if (latestData != null)
                {
                    _logger.LogDebug("=== Данные найдены ===");
                    
                    var jsonOptions = new System.Text.Json.JsonSerializerOptions 
                    { 
                        WriteIndented = true,
                        PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase
                    };

                    ViewData["PostName"] = postName;
                    ViewData["PostAddress"] = postAddress;
                    ViewData["SensorTypeName"] = sensorTypeName;
                    ViewData["EndPointsName"] = endPointsName;
                    ViewData["ReceivedAt"] = latestData.ReceivedAt;

                    return PartialView("_SensorData", latestData);
                }
                else
                {
                    _logger.LogDebug("=== Данные НЕ найдены === В таблице для типа {Type} нет записей для sensorId = {SensorId}", sensorTypeName, sensorId);
                }
                
                _logger.LogDebug("===========================");
                
                if (latestData == null)
                {
                    return PartialView("_SensorData", null);
                }
                
                ViewBag.SensorTypeId = sensorTypeId;
                ViewBag.SensorId = sensorId;
                ViewBag.SensorTypeName = sensorTypeName;
                
                return PartialView("_SensorData", latestData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "!!! ОШИБКА в GetLatestSensorData for sensorId: {SensorId}", sensorId);
                return Content($"Ошибка: {ex.Message}");
            }
        }

        [HttpGet]
        public async Task<IActionResult> TestSensorData(int sensorId)
        {
            try
            {
                var results = new List<string>();
                
                var iwsCount = await _context.IWSData.CountAsync(d => d.SensorId == sensorId);
                results.Add($"IWSData: {iwsCount} записей");
                
                var dspdCount = await _context.DSPDData.CountAsync(d => d.SensorId == sensorId);
                results.Add($"DSPDData: {dspdCount} записей");
                
                var dustCount = await _context.DustData.CountAsync(d => d.SensorId == sensorId);
                results.Add($"DustData: {dustCount} записей");
                
                var dovCount = await _context.DOVData.CountAsync(d => d.SensorId == sensorId);
                results.Add($"DOVData: {dovCount} записей");
                
                var mueksCount = await _context.MUEKSData.CountAsync(d => d.SensorId == sensorId);
                results.Add($"MUEKSData: {mueksCount} записей");
                
                return Content(string.Join("\n", results));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in TestSensorData for sensorId: {SensorId}", sensorId);
                return Content($"Ошибка: {ex.Message}");
            }
        }
    }
}