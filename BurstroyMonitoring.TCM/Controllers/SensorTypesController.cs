using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BurstroyMonitoring.Data;
using BurstroyMonitoring.Data.Models;

namespace BurstroyMonitoring.TCM.Controllers
{
    public class SensorTypesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SensorTypesController(ApplicationDbContext context)
        {
            _context = context;
        }

       // GET: SensorTypes
        public async Task<IActionResult> Index(
            string sortBy = "Id",
            bool sortDesc = true,
            string search = "")
        {
            // Включаем датчики для подсчета
            var query = _context.SensorTypes
                .Include(st => st.Sensors) // Включаем для подсчета
                .AsQueryable();

            // Применяем поиск, если есть
            if (!string.IsNullOrEmpty(search))
            {
                search = search.ToLower();
                query = query.Where(st => 
                    st.SensorTypeName.ToLower().Contains(search) ||
                    st.Description.ToLower().Contains(search));
                
                ViewBag.Search = search;
            }

            // Применяем сортировку
            switch (sortBy?.ToLower())
            {
                case "sensortypename":
                case "name":
                    query = sortDesc ? query.OrderByDescending(st => st.SensorTypeName) 
                                    : query.OrderBy(st => st.SensorTypeName);
                    break;
                case "description":
                    query = sortDesc ? query.OrderByDescending(st => st.Description) 
                                    : query.OrderBy(st => st.Description);
                    break;
                case "created":
                case "createdat":
                    query = sortDesc ? query.OrderByDescending(st => st.CreatedAt) 
                                    : query.OrderBy(st => st.CreatedAt);
                    break;
                case "sensors":
                case "sensorscount":
                case "count":
                    // Сортировка по количеству датчиков
                    query = sortDesc ? 
                        query.OrderByDescending(st => st.Sensors.Count) :
                        query.OrderBy(st => st.Sensors.Count);
                    break;
                default: // "id" или по умолчанию
                    query = sortDesc ? query.OrderByDescending(st => st.Id) 
                                    : query.OrderBy(st => st.Id);
                    sortBy = "Id";
                    break;
            }

            // Сохраняем параметры сортировки в ViewBag
            ViewBag.SortBy = sortBy;
            ViewBag.SortDesc = sortDesc;

            var sensorTypes = await query.ToListAsync();
            return View(sensorTypes);
        }

       // GET: SensorTypes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // Загружаем сам тип датчика
            var sensorType = await _context.SensorTypes
                .FirstOrDefaultAsync(m => m.Id == id);
            
            if (sensorType == null)
            {
                return NotFound();
            }

            // Загружаем датчики отдельно
            var sensors = await _context.Sensors
                .Where(s => s.SensorTypeId == id)
                .Include(s => s.MonitoringPost) // Загружаем посты
                .Take(10) // Ограничиваем количество для отображения
                .ToListAsync();

            // Передаем в ViewBag
            ViewBag.Sensors = sensors;
            ViewBag.TotalSensorCount = await _context.Sensors
                .Where(s => s.SensorTypeId == id)
                .CountAsync();

            return View(sensorType);
        }

        // GET: SensorTypes/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: SensorTypes/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,SensorTypeName,Description")] SensorType sensorType)
        {
            if (ModelState.IsValid)
            {
                sensorType.CreatedAt = DateTime.UtcNow;
                _context.Add(sensorType);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(sensorType);
        }

        // GET: SensorTypes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var sensorType = await _context.SensorTypes.FindAsync(id);
            if (sensorType == null)
            {
                return NotFound();
            }
            return View(sensorType);
        }

        // POST: SensorTypes/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,SensorTypeName,Description,CreatedAt")] SensorType sensorType)
        {
            if (id != sensorType.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Преобразуем CreatedAt в UTC, если оно не в UTC
                    if (sensorType.CreatedAt.HasValue && sensorType.CreatedAt.Value.Kind != DateTimeKind.Utc)
                    {
                        sensorType.CreatedAt = sensorType.CreatedAt.Value.ToUniversalTime();
                    }
                    
                    _context.Update(sensorType);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SensorTypeExists(sensorType.Id))
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
            return View(sensorType);
        }

        // GET: SensorTypes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var sensorType = await _context.SensorTypes
                .FirstOrDefaultAsync(m => m.Id == id);
            if (sensorType == null)
            {
                return NotFound();
            }

            return View(sensorType);
        }

        // POST: SensorTypes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var sensorType = await _context.SensorTypes.FindAsync(id);
            if (sensorType != null)
            {
                _context.SensorTypes.Remove(sensorType);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool SensorTypeExists(int id)
        {
            return _context.SensorTypes.Any(e => e.Id == id);
        }
    }
}