using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BurstroyMonitoring.Data;
using BurstroyMonitoring.Data.Models;

namespace BurstroyMonitoring.TCM.Controllers
{
    public class SensorsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SensorsController(ApplicationDbContext context)
        {
            _context = context;
        }

    // GET: Sensors
    public async Task<IActionResult> Index(
        string sortBy = "Id",
        bool sortDesc = true,
        string search = "",
        int? sensorTypeId = null,
        int? monitoringPostId = null,
        List<int> selectedSensorTypes = null,
        List<int> selectedMonitoringPosts = null)
    {
        var query = _context.Sensors
            .Include(s => s.SensorType)
            .Include(s => s.MonitoringPost)
            .AsQueryable();

        // Фильтрация по параметрам из ссылки
        if (sensorTypeId.HasValue)
        {
            query = query.Where(s => s.SensorTypeId == sensorTypeId.Value);
            ViewBag.SelectedSensorTypeId = sensorTypeId.Value;
        }

        if (monitoringPostId.HasValue)
        {
            query = query.Where(s => s.MonitoringPostId == monitoringPostId.Value);
            ViewBag.SelectedMonitoringPostId = monitoringPostId.Value;
        }

        // Фильтрация по выбранным чекбоксам
        if (selectedSensorTypes != null && selectedSensorTypes.Any())
        {
            query = query.Where(s => selectedSensorTypes.Contains(s.SensorTypeId));
            ViewBag.SelectedSensorTypeIds = selectedSensorTypes;
        }

        if (selectedMonitoringPosts != null && selectedMonitoringPosts.Any())
        {
            query = query.Where(s => selectedMonitoringPosts.Contains(s.MonitoringPostId.Value));
            ViewBag.SelectedMonitoringPostIds = selectedMonitoringPosts;
        }

        // Поиск по серийному номеру и названию конечной точки
        if (!string.IsNullOrEmpty(search))
        {
            search = search.ToLower();
            query = query.Where(s => 
                s.SerialNumber.ToLower().Contains(search) ||
                s.EndPointsName.ToLower().Contains(search) ||
                (s.SensorType != null && s.SensorType.SensorTypeName.ToLower().Contains(search)) ||
                (s.MonitoringPost != null && s.MonitoringPost.Name.ToLower().Contains(search)));
            
            ViewBag.Search = search;
        }

        // Применяем сортировку
        switch (sortBy?.ToLower())
        {
            case "serialnumber":
                query = sortDesc ? query.OrderByDescending(s => s.SerialNumber) 
                                : query.OrderBy(s => s.SerialNumber);
                break;
            case "sensortype":
            case "type":
                query = sortDesc ? 
                    query.OrderByDescending(s => s.SensorType != null ? s.SensorType.SensorTypeName : "") :
                    query.OrderBy(s => s.SensorType != null ? s.SensorType.SensorTypeName : "");
                break;
            case "monitoringpost":
            case "post":
                query = sortDesc ? 
                    query.OrderByDescending(s => s.MonitoringPost != null ? s.MonitoringPost.Name : "") :
                    query.OrderBy(s => s.MonitoringPost != null ? s.MonitoringPost.Name : "");
                break;
            case "url":
                query = sortDesc ? query.OrderByDescending(s => s.Url) 
                                : query.OrderBy(s => s.Url);
                break;
            case "isactive":
            case "active":
                query = sortDesc ? query.OrderByDescending(s => s.IsActive) 
                                : query.OrderBy(s => s.IsActive);
                break;
            case "lastactivity":
            case "lastactivityutc":
                query = sortDesc ? query.OrderByDescending(s => s.LastActivityUTC) 
                                : query.OrderBy(s => s.LastActivityUTC);
                break;
            case "checkinterval":
            case "checkintervalseconds":
                query = sortDesc ? query.OrderByDescending(s => s.CheckIntervalSeconds) 
                                : query.OrderBy(s => s.CheckIntervalSeconds);
                break;
            case "created":
            case "createdat":
                query = sortDesc ? query.OrderByDescending(s => s.CreatedAt) 
                                : query.OrderBy(s => s.CreatedAt);
                break;
            default: // "id" или по умолчанию
                query = sortDesc ? query.OrderByDescending(s => s.Id) 
                                : query.OrderBy(s => s.Id);
                sortBy = "Id";
                break;
        }

        // Получаем уникальные значения для фильтров
        var sensorTypes = await _context.Sensors
            .Where(s => s.SensorTypeId != null)
            .Select(s => new { s.SensorTypeId, s.SensorType.SensorTypeName })
            .Distinct()
            .OrderBy(x => x.SensorTypeName)
            .ToListAsync();

        var monitoringPosts = await _context.Sensors
            .Where(s => s.MonitoringPostId != null)
            .Select(s => new { s.MonitoringPostId, s.MonitoringPost.Name })
            .Distinct()
            .OrderBy(x => x.Name)
            .ToListAsync();

        ViewBag.SensorTypes = sensorTypes;
        ViewBag.MonitoringPosts = monitoringPosts;
        ViewBag.AllSensorCount = await _context.Sensors.CountAsync();
        
        // Сохраняем параметры сортировки
        ViewBag.SortBy = sortBy;
        ViewBag.SortDesc = sortDesc;

        var sensors = await query.ToListAsync();
        return View(sensors);
    }

        // GET: Sensors/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var sensor = await _context.Sensors
                .Include(s => s.SensorType)
                .Include(s => s.MonitoringPost)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (sensor == null)
            {
                return NotFound();
            }

            return View(sensor);
        }

        // GET: Sensors/Create
        public IActionResult Create()
        {
            ViewData["MonitoringPostId"] = new SelectList(_context.MonitoringPosts, "Id", "Name");
            ViewData["SensorTypeId"] = new SelectList(_context.SensorTypes, "Id", "SensorTypeName");
            return View();
        }

        // POST: Sensors/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,SensorTypeId,Longitude,Latitude,SerialNumber,EndPointsName,Url,CheckIntervalSeconds,LastActivityUTC,IsActive,MonitoringPostId")] Sensor sensor)
        {
            if (ModelState.IsValid)
            {
                sensor.CreatedAt = DateTime.UtcNow;
                
                // Преобразуем LastActivityUTC в UTC, если оно указано
                if (sensor.LastActivityUTC.HasValue && sensor.LastActivityUTC.Value.Kind != DateTimeKind.Utc)
                {
                    sensor.LastActivityUTC = sensor.LastActivityUTC.Value.ToUniversalTime();
                }
                
                _context.Add(sensor);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["MonitoringPostId"] = new SelectList(_context.MonitoringPosts, "Id", "Name", sensor.MonitoringPostId);
            ViewData["SensorTypeId"] = new SelectList(_context.SensorTypes, "Id", "SensorTypeName", sensor.SensorTypeId);
            return View(sensor);
        }

        // GET: Sensors/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var sensor = await _context.Sensors.FindAsync(id);
            if (sensor == null)
            {
                return NotFound();
            }
            ViewData["MonitoringPostId"] = new SelectList(_context.MonitoringPosts, "Id", "Name", sensor.MonitoringPostId);
            ViewData["SensorTypeId"] = new SelectList(_context.SensorTypes, "Id", "SensorTypeName", sensor.SensorTypeId);
            return View(sensor);
        }

        // POST: Sensors/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,SensorTypeId,Longitude,Latitude,SerialNumber,EndPointsName,Url,CheckIntervalSeconds,LastActivityUTC,CreatedAt,IsActive,MonitoringPostId")] Sensor sensor)
        {
            if (id != sensor.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Преобразуем CreatedAt в UTC, если оно не в UTC
                    if (sensor.CreatedAt.HasValue && sensor.CreatedAt.Value.Kind != DateTimeKind.Utc)
                    {
                        sensor.CreatedAt = sensor.CreatedAt.Value.ToUniversalTime();
                    }
                    
                    // Преобразуем LastActivityUTC в UTC, если оно указано
                    if (sensor.LastActivityUTC.HasValue && sensor.LastActivityUTC.Value.Kind != DateTimeKind.Utc)
                    {
                        sensor.LastActivityUTC = sensor.LastActivityUTC.Value.ToUniversalTime();
                    }
                    
                    _context.Update(sensor);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SensorExists(sensor.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["MonitoringPostId"] = new SelectList(_context.MonitoringPosts, "Id", "Name", sensor.MonitoringPostId);
            ViewData["SensorTypeId"] = new SelectList(_context.SensorTypes, "Id", "SensorTypeName", sensor.SensorTypeId);
            return View(sensor);
        }

        // GET: Sensors/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var sensor = await _context.Sensors
                .Include(s => s.SensorType)
                .Include(s => s.MonitoringPost)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (sensor == null)
            {
                return NotFound();
            }

            return View(sensor);
        }

        // POST: Sensors/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var sensor = await _context.Sensors.FindAsync(id);
            if (sensor != null)
            {
                _context.Sensors.Remove(sensor);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool SensorExists(int id)
        {
            return _context.Sensors.Any(e => e.Id == id);
        }
    }
}