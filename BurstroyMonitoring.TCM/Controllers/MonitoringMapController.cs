using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BurstroyMonitoring.Data;
using System.Linq;
using System.Threading.Tasks;

namespace BurstroyMonitoring.TCM.Controllers
{
    public class MonitoringMapController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MonitoringMapController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
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

        [HttpGet]
        public async Task<IActionResult> GetMapData()
        {
            // Получаем все посты с координатами (и активные и неактивные)
            var posts = await _context.MonitoringPosts
                .Where(p => p.Latitude != null && p.Longitude != null) // Убираем фильтр p.IsActive
                .Select(p => new
                {
                    Id = p.Id,
                    Type = "post",
                    Name = p.Name,
                    Description = p.Description,
                    Latitude = p.Latitude,
                    Longitude = p.Longitude,
                    IsMobile = p.IsMobile,
                    IsActive = p.IsActive,
                    SensorCount = p.Sensors.Count // Считаем все датчики
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
                    CheckIntervalSeconds = s.CheckIntervalSeconds,
                    MonitoringPostId = s.MonitoringPostId,
                    IsActive = s.IsActive,
                    SensorTypeName = s.SensorType != null ? s.SensorType.SensorTypeName : ""
                })
                .ToListAsync();

            return Json(new { posts, sensors });
        }

        [HttpGet]
        public async Task<IActionResult> GetDetails(int id, string type)
        {
            if (type == "post")
            {
                var post = await _context.MonitoringPosts
                    .Include(p => p.Sensors) // Убираем фильтр Where(s => s.IsActive)
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (post == null)
                    return NotFound();

                return PartialView("_PostDetails", post);
            }
           
            return NotFound();
        }

        [HttpGet]
        public async Task<IActionResult> SearchPosts(string query)
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

        [HttpGet]
        public async Task<IActionResult> GetSensorData(int postId)
        {
            var sensors = await _context.Sensors
                .Include(s => s.SensorType)
                .Where(s => s.MonitoringPostId == postId) // Убираем фильтр s.IsActive
                .Select(s => new 
                {
                    Id = s.Id,
                    EndPointsName = s.EndPointsName,
                    SerialNumber = s.SerialNumber,
                    Url = s.Url,
                    IsActive = s.IsActive,
                    SensorTypeId = s.SensorTypeId,
                    SensorTypeName = s.SensorType != null ? s.SensorType.SensorTypeName : ""
                })
                .ToListAsync();

            return PartialView("_SensorList", sensors);
        }

        [HttpGet]
        public async Task<IActionResult> GetLatestSensorData(int sensorId, int sensorTypeId)
        {
            try
            {
                // Получаем имя типа датчика из базы данных
                var sensorType = await _context.SensorTypes
                    .FirstOrDefaultAsync(st => st.Id == sensorTypeId);
                
                string sensorTypeName = sensorType?.SensorTypeName ?? "Unknown";
                
                Console.WriteLine($"=== ЗАПРОС ДАННЫХ ДАТЧИКА ===");
                Console.WriteLine($"SensorId: {sensorId}, SensorTypeId: {sensorTypeId}, SensorTypeName: {sensorTypeName}");
                Console.WriteLine($"Timestamp: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");

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
                                dspdDirect.Shake,
                                dspdDirect.UPower,
                                dspdDirect.TemperatureCase,
                                dspdDirect.TemperatureRoad,
                                dspdDirect.HeightH2O,
                                dspdDirect.HeightIce,
                                dspdDirect.HeightSnow,
                                dspdDirect.PercentICE,
                                dspdDirect.PercentPGM,
                                dspdDirect.RoadStatus,
                                dspdDirect.AngleToRoad,
                                dspdDirect.TemperatureFreezePGM,
                                dspdDirect.NeedCalibration,
                                dspdDirect.GPSLatitude,
                                dspdDirect.GPSLongitude,
                                IsGpsValid = dspdDirect.IsGpsValid,
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
                                iwsDirect.PressureQNHHPa,
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
                                iwsDirect.Latitude,
                                iwsDirect.Longitude,
                                iwsDirect.Altitude,
                                iwsDirect.KSP,
                                iwsDirect.GPSSpeed,
                                iwsDirect.AccelerationStDev,
                                iwsDirect.Roll,
                                iwsDirect.Pitch,
                                iwsDirect.WeAreFine,
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
                                dovDirect.BrightFlag,
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
                                dustDirect.LaserStatus,
                                dustDirect.SupplyVoltage,
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
                    Console.WriteLine("=== Данные найдены ===");
                    
                    var jsonOptions = new System.Text.Json.JsonSerializerOptions 
                    { 
                        WriteIndented = true,
                        PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase
                    };
                    
                    string json = System.Text.Json.JsonSerializer.Serialize(latestData, jsonOptions);
                    Console.WriteLine(json);
                }
                else
                {
                    Console.WriteLine($"=== Данные НЕ найдены ===");
                    Console.WriteLine($"В таблице для типа {sensorTypeName} нет записей для sensorId = {sensorId}");
                }
                
                Console.WriteLine("===========================");
                Console.WriteLine();
                
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
                Console.WriteLine($"!!! ОШИБКА в GetLatestSensorData: {ex.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");
                return Content($"Ошибка: {ex.Message}");
            }
        }

        [HttpGet]
        public async Task<IActionResult> TestSensorData(int sensorId)
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
    }
}