using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BurstroyMonitoring.Data;
using BurstroyMonitoring.Data.Models;

namespace BurstroyMonitoring.TCM.Controllers
{
    public class PuidsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<PuidsController> _logger;

        public PuidsController(ApplicationDbContext context, ILogger<PuidsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: Puids
        public async Task<IActionResult> Index(string sortBy = "Id", bool sortDesc = true)
        {
            try
            {
                var query = _context.Puids
                    .Include(p => p.MonitoringPost)
                    .AsQueryable();

                // Применяем сортировку
                switch (sortBy?.ToLower())
                {
                    case "serialnumber":
                        query = sortDesc ? query.OrderByDescending(p => p.SerialNumber) : query.OrderBy(p => p.SerialNumber);
                        break;
                    case "endpointsname":
                        query = sortDesc ? query.OrderByDescending(p => p.EndPointsName) : query.OrderBy(p => p.EndPointsName);
                        break;
                    case "monitoringpost":
                        query = sortDesc ? query.OrderByDescending(p => p.MonitoringPost != null ? p.MonitoringPost.Name : "") : query.OrderBy(p => p.MonitoringPost != null ? p.MonitoringPost.Name : "");
                        break;
                    case "url":
                        query = sortDesc ? query.OrderByDescending(p => p.Url) : query.OrderBy(p => p.Url);
                        break;
                    case "interval":
                        query = sortDesc ? query.OrderByDescending(p => p.IntervalSeconds) : query.OrderBy(p => p.IntervalSeconds);
                        break;
                    case "isactive":
                        query = sortDesc ? query.OrderByDescending(p => p.IsActive) : query.OrderBy(p => p.IsActive);
                        break;
                    case "lastactivity":
                        query = sortDesc ? query.OrderByDescending(p => p.LastActivityUTC) : query.OrderBy(p => p.LastActivityUTC);
                        break;
                    default:
                        query = sortDesc ? query.OrderByDescending(p => p.Id) : query.OrderBy(p => p.Id);
                        sortBy = "Id";
                        break;
                }

                ViewBag.SortBy = sortBy;
                ViewBag.SortDesc = sortDesc;

                return View(await query.ToListAsync());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in PuidsController.Index");
                return View(new List<Puid>());
            }
        }

        // GET: Puids/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            try
            {
                if (id == null) return NotFound();

                var puid = await _context.Puids
                    .Include(p => p.MonitoringPost)
                    .FirstOrDefaultAsync(m => m.Id == id);
                
                if (puid == null) return NotFound();

                return View(puid);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in PuidsController.Details for id: {Id}", id);
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Puids/Create
        public IActionResult Create()
        {
            try
            {
                ViewData["MonitoringPostId"] = new SelectList(_context.MonitoringPosts, "Id", "Name");
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in PuidsController.Create (GET)");
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Puids/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,SensorType,MonitoringPostId,Longitude,Latitude,SerialNumber,EndPointsName,IntervalSeconds,Url,IsActive")] Puid puid)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    puid.CreatedAt = DateTime.UtcNow;
                    _context.Add(puid);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Puid created successfully: {SerialNumber}", puid.SerialNumber);
                    return RedirectToAction(nameof(Index));
                }
                ViewData["MonitoringPostId"] = new SelectList(_context.MonitoringPosts, "Id", "Name", puid.MonitoringPostId);
                return View(puid);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in PuidsController.Create (POST)");
                ModelState.AddModelError("", "Ошибка при создании PUID");
                ViewData["MonitoringPostId"] = new SelectList(_context.MonitoringPosts, "Id", "Name", puid.MonitoringPostId);
                return View(puid);
            }
        }

        // GET: Puids/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            try
            {
                if (id == null) return NotFound();

                var puid = await _context.Puids.FindAsync(id);
                if (puid == null) return NotFound();
                
                ViewData["MonitoringPostId"] = new SelectList(_context.MonitoringPosts, "Id", "Name", puid.MonitoringPostId);
                return View(puid);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in PuidsController.Edit (GET) for id: {Id}", id);
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Puids/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,SensorType,MonitoringPostId,Longitude,Latitude,SerialNumber,EndPointsName,IntervalSeconds,Url,IsActive,CreatedAt,LastActivityUTC")] Puid puid)
        {
            if (id != puid.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    // Исправляем Kind для PostgreSQL
                    puid.CreatedAt = DateTime.SpecifyKind(puid.CreatedAt, DateTimeKind.Utc);
                    if (puid.LastActivityUTC.HasValue)
                    {
                        puid.LastActivityUTC = DateTime.SpecifyKind(puid.LastActivityUTC.Value, DateTimeKind.Utc);
                    }

                    _context.Update(puid);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Puid updated successfully: {SerialNumber}", puid.SerialNumber);
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    if (!PuidExists(puid.Id)) return NotFound();
                    else
                    {
                        _logger.LogError(ex, "Concurrency error in PuidsController.Edit for id: {Id}", id);
                        throw;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in PuidsController.Edit (POST) for id: {Id}", id);
                    ModelState.AddModelError("", "Ошибка при сохранении изменений");
                }
            }
            ViewData["MonitoringPostId"] = new SelectList(_context.MonitoringPosts, "Id", "Name", puid.MonitoringPostId);
            return View(puid);
        }
        // GET: Puids/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            try
            {
                if (id == null) return NotFound();

                var puid = await _context.Puids
                    .Include(p => p.MonitoringPost)
                    .FirstOrDefaultAsync(m => m.Id == id);
                
                if (puid == null) return NotFound();

                return View(puid);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in PuidsController.Delete (GET) for id: {Id}", id);
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Puids/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var puid = await _context.Puids.FindAsync(id);
                if (puid != null)
                {
                    _context.Puids.Remove(puid);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Puid deleted successfully: {Id}", id);
                }
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in PuidsController.DeleteConfirmed for id: {Id}", id);
                TempData["ErrorMessage"] = "Ошибка при удалении PUID";
                return RedirectToAction(nameof(Index));
            }
        }
        private bool PuidExists(int id)
        {
            return _context.Puids.Any(e => e.Id == id);
        }
    }
}
