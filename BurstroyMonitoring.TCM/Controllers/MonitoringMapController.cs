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
            var posts = await _context.MonitoringPosts
                .Where(p => p.IsActive)
                .Include(p => p.Sensors.Where(s => s.IsActive))
                .ToListAsync();

            var sensorsWithoutPost = await _context.Sensors
                .Where(s => s.IsActive && s.MonitoringPostId == null && s.Latitude != null && s.Longitude != null)
                .ToListAsync();

            ViewData["TotalPosts"] = posts.Count;
            ViewData["TotalSensors"] = posts.Sum(p => p.Sensors.Count) + sensorsWithoutPost.Count;

            return View(posts);
        }

        [HttpGet]
        public async Task<IActionResult> GetMapData()
        {
            var posts = await _context.MonitoringPosts
                .Where(p => p.IsActive && p.Latitude != null && p.Longitude != null)
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
                    SensorCount = p.Sensors.Count(s => s.IsActive)
                })
                .ToListAsync();

            var sensors = await _context.Sensors
                .Where(s => s.IsActive && s.Latitude != null && s.Longitude != null)
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
                    IsActive = s.IsActive
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
                    .Include(p => p.Sensors.Where(s => s.IsActive))
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

            // Поиск без учета регистра
            var posts = await _context.MonitoringPosts
                .Where(p => EF.Functions.ILike(p.Name, $"%{query}%") && p.Latitude != null && p.Longitude != null)
                .Select(p => new
                {
                    p.Id,
                    p.Name,
                    p.Latitude,
                    p.Longitude
                })
                .Take(10)
                .ToListAsync();

            return Json(posts);
        }

        [HttpGet]
        public async Task<IActionResult> GetSensorData(int postId)
        {
            var sensors = await _context.Sensors
                .Where(s => s.MonitoringPostId == postId && s.IsActive)
                .ToListAsync();

            return PartialView("_SensorList", sensors);
        }

        [HttpGet]
        public async Task<IActionResult> GetLatestSensorData(int sensorId, int sensorTypeId)
        {
            dynamic latestData = null;
            
            switch (sensorTypeId)
            {
                case 1: // IWSData
                    latestData = await _context.Set<dynamic>()
                        .FromSqlRaw($"SELECT * FROM \"IWSData\" WHERE \"SensorId\" = {sensorId} ORDER BY \"ReceivedAt\" DESC LIMIT 1")
                        .FirstOrDefaultAsync();
                    break;
                    
                case 2: // DSPDData
                    latestData = await _context.Set<dynamic>()
                        .FromSqlRaw($"SELECT * FROM \"DSPDData\" WHERE \"SensorId\" = {sensorId} ORDER BY \"ReceivedAt\" DESC LIMIT 1")
                        .FirstOrDefaultAsync();
                    break;
                    
                case 3: // DustData
                    latestData = await _context.Set<dynamic>()
                        .FromSqlRaw($"SELECT * FROM \"DustData\" WHERE \"SensorId\" = {sensorId} ORDER BY \"ReceivedAt\" DESC LIMIT 1")
                        .FirstOrDefaultAsync();
                    break;
                    
                case 4: // DOVData
                    latestData = await _context.Set<dynamic>()
                        .FromSqlRaw($"SELECT * FROM \"DOVData\" WHERE \"SensorId\" = {sensorId} ORDER BY \"ReceivedAt\" DESC LIMIT 1")
                        .FirstOrDefaultAsync();
                    break;
                    
                case 5: // MUEKSData
                    latestData = await _context.Set<dynamic>()
                        .FromSqlRaw($"SELECT * FROM \"MUEKSData\" WHERE \"SensorId\" = {sensorId} ORDER BY \"ReceivedAt\" DESC LIMIT 1")
                        .FirstOrDefaultAsync();
                    break;
            }
            
            if (latestData == null)
                return Json(new object());
            
            return Json(latestData);
        }
    }
}