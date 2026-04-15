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

        public PuidsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Puids
        public async Task<IActionResult> Index(string sortBy = "Id", bool sortDesc = true)
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

        // GET: Puids/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var puid = await _context.Puids
                .Include(p => p.MonitoringPost)
                .FirstOrDefaultAsync(m => m.Id == id);
            
            if (puid == null) return NotFound();

            return View(puid);
        }

        // GET: Puids/Create
        public IActionResult Create()
        {
            ViewData["MonitoringPostId"] = new SelectList(_context.MonitoringPosts, "Id", "Name");
            return View();
        }

        // POST: Puids/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,SensorType,MonitoringPostId,Longitude,Latitude,SerialNumber,EndPointsName,IntervalSeconds,Url,IsActive")] Puid puid)
        {
            if (ModelState.IsValid)
            {
                puid.CreatedAt = DateTime.UtcNow;
                _context.Add(puid);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["MonitoringPostId"] = new SelectList(_context.MonitoringPosts, "Id", "Name", puid.MonitoringPostId);
            return View(puid);
        }

        // GET: Puids/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var puid = await _context.Puids.FindAsync(id);
            if (puid == null) return NotFound();
            
            ViewData["MonitoringPostId"] = new SelectList(_context.MonitoringPosts, "Id", "Name", puid.MonitoringPostId);
            return View(puid);
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
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PuidExists(puid.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["MonitoringPostId"] = new SelectList(_context.MonitoringPosts, "Id", "Name", puid.MonitoringPostId);
            return View(puid);
        }
        // GET: Puids/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var puid = await _context.Puids
                .Include(p => p.MonitoringPost)
                .FirstOrDefaultAsync(m => m.Id == id);
            
            if (puid == null) return NotFound();

            return View(puid);
        }

        // POST: Puids/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var puid = await _context.Puids.FindAsync(id);
            if (puid != null)
            {
                _context.Puids.Remove(puid);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PuidExists(int id)
        {
            return _context.Puids.Any(e => e.Id == id);
        }
    }
}
