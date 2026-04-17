using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BurstroyMonitoring.Data;
using BurstroyMonitoring.Data.Models;

namespace BurstroyMonitoring.TCM.Controllers
{
    public class MonitoringPostsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<MonitoringPostsController> _logger;

        public MonitoringPostsController(ApplicationDbContext context, ILogger<MonitoringPostsController> logger)
        {
            _context = context;
            _logger = logger;
        }

       // GET: MonitoringPosts
        public async Task<IActionResult> Index(
            string sortBy = "Id",
            bool sortDesc = true,
            string search = "")
        {
            try
            {
                var query = _context.MonitoringPosts.AsQueryable();

                // Применяем поиск, если есть
                if (!string.IsNullOrEmpty(search))
                {
                    search = search.ToLower();
                    query = query.Where(mp => 
                        mp.Name.ToLower().Contains(search) ||
                        (mp.Address != null && mp.Address.ToLower().Contains(search)) ||
                        (mp.Description != null && mp.Description.ToLower().Contains(search)));
                    
                    ViewBag.Search = search;
                }

                // Применяем сортировку
                switch (sortBy?.ToLower())
                {
                    case "name":
                        query = sortDesc ? query.OrderByDescending(mp => mp.Name) 
                                        : query.OrderBy(mp => mp.Name);
                        break;
                    case "address":
                        query = sortDesc ? query.OrderByDescending(mp => mp.Address ?? "") 
                                        : query.OrderBy(mp => mp.Address ?? "");
                        break;
                    case "description":
                        query = sortDesc ? query.OrderByDescending(mp => mp.Description ?? "") 
                                        : query.OrderBy(mp => mp.Description ?? "");
                        break;
                    case "coordinates":
                        // Сортировка по наличию координат, затем по долготе
                        query = sortDesc ? 
                            query.OrderByDescending(mp => mp.Longitude.HasValue)
                                .ThenByDescending(mp => mp.Longitude) :
                            query.OrderBy(mp => mp.Longitude.HasValue)
                                .ThenBy(mp => mp.Longitude);
                        break;
                    case "type":
                        query = sortDesc ? query.OrderByDescending(mp => mp.IsMobile) 
                                        : query.OrderBy(mp => mp.IsMobile);
                        break;
                    case "status":
                        query = sortDesc ? query.OrderByDescending(mp => mp.IsActive) 
                                        : query.OrderBy(mp => mp.IsActive);
                        break;
                    case "created":
                        query = sortDesc ? query.OrderByDescending(mp => mp.CreatedAt) 
                                        : query.OrderBy(mp => mp.CreatedAt);
                        break;
                    case "pollinginterval":
                        query = sortDesc ? query.OrderByDescending(mp => mp.PollingIntervalSeconds) 
                                        : query.OrderBy(mp => mp.PollingIntervalSeconds);
                        break;
                    default: // "id" или по умолчанию
                        query = sortDesc ? query.OrderByDescending(mp => mp.Id) 
                                        : query.OrderBy(mp => mp.Id);
                        sortBy = "Id";
                        break;
                }

                // Сохраняем параметры сортировки в ViewBag
                ViewBag.SortBy = sortBy;
                ViewBag.SortDesc = sortDesc;

                var monitoringPosts = await query.ToListAsync();
                return View(monitoringPosts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in MonitoringPostsController.Index");
                return View(new List<MonitoringPost>());
            }
        }

        // GET: MonitoringPosts/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            try
            {
                if (id == null)
                {
                    return NotFound();
                }

                // Загружаем пост мониторинга вместе с датчиками
                var monitoringPost = await _context.MonitoringPosts
                    .Include(mp => mp.Sensors) // Добавляем загрузку датчиков
                    .FirstOrDefaultAsync(m => m.Id == id);
                
                if (monitoringPost == null)
                {
                    return NotFound();
                }

                return View(monitoringPost);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in MonitoringPostsController.Details for id: {Id}", id);
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: MonitoringPosts/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: MonitoringPosts/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Address,Description,Longitude,Latitude,IsMobile,IsActive,PollingIntervalSeconds")] MonitoringPost monitoringPost)
        {
            try
            {
                _logger.LogDebug("=== !СОЗДАНИЕ ПОСТА === Name: {Name}, Longitude: {Longitude}, Latitude: {Latitude}", 
                    monitoringPost.Name, monitoringPost.Longitude, monitoringPost.Latitude);
                
                // Вывести все ошибки ModelState
                foreach (var error in ModelState)
                {
                    if (error.Value.Errors.Count > 0)
                    {
                        _logger.LogDebug("Ошибка в поле {Key}: {Errors}", 
                            error.Key, string.Join(", ", error.Value.Errors.Select(e => e.ErrorMessage)));
                    }
                }
                
                if (ModelState.IsValid)
                {
                    monitoringPost.CreatedAt = DateTime.UtcNow;
                    monitoringPost.UpdatedAt = DateTime.UtcNow;
                    _context.Add(monitoringPost);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Monitoring post created successfully: {Name}", monitoringPost.Name);
                    return RedirectToAction(nameof(Index));
                }
                
                _logger.LogWarning("Validation failed for creating monitoring post: {Name}", monitoringPost.Name);
                return View(monitoringPost);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in MonitoringPostsController.Create (POST)");
                ModelState.AddModelError("", "Ошибка при создании поста");
                return View(monitoringPost);
            }
        }

        // GET: MonitoringPosts/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            try
            {
                if (id == null)
                {
                    return NotFound();
                }

                var monitoringPost = await _context.MonitoringPosts.FindAsync(id);
                if (monitoringPost == null)
                {
                    return NotFound();
                }
                return View(monitoringPost);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in MonitoringPostsController.Edit (GET) for id: {Id}", id);
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: MonitoringPosts/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Address,Description,Longitude,Latitude,IsMobile,IsActive,CreatedAt,PollingIntervalSeconds")] MonitoringPost monitoringPost)
        {
            if (id != monitoringPost.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingPost = await _context.MonitoringPosts.FindAsync(id);
                    if (existingPost == null)
                    {
                        return NotFound();
                    }

                    // Обновляем только нужные поля
                    existingPost.Name = monitoringPost.Name;
                    existingPost.Address = monitoringPost.Address;
                    existingPost.Description = monitoringPost.Description;
                    existingPost.Longitude = monitoringPost.Longitude;
                    existingPost.Latitude = monitoringPost.Latitude;
                    existingPost.IsMobile = monitoringPost.IsMobile;
                    existingPost.IsActive = monitoringPost.IsActive;
                    existingPost.PollingIntervalSeconds = monitoringPost.PollingIntervalSeconds;
                    existingPost.UpdatedAt = DateTime.UtcNow;

                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Monitoring post updated successfully: {Name}", monitoringPost.Name);
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    if (!MonitoringPostExists(monitoringPost.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        _logger.LogError(ex, "Concurrency error in MonitoringPostsController.Edit for id: {Id}", id);
                        throw;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in MonitoringPostsController.Edit (POST) for id: {Id}", id);
                    ModelState.AddModelError("", "Ошибка при сохранении изменений");
                }
            }
            return View(monitoringPost);
        }

        // GET: MonitoringPosts/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            try
            {
                if (id == null)
                {
                    return NotFound();
                }

                var monitoringPost = await _context.MonitoringPosts
                    .FirstOrDefaultAsync(m => m.Id == id);
                if (monitoringPost == null)
                {
                    return NotFound();
                }

                return View(monitoringPost);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in MonitoringPostsController.Delete (GET) for id: {Id}", id);
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: MonitoringPosts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var monitoringPost = await _context.MonitoringPosts.FindAsync(id);
                if (monitoringPost != null)
                {
                    _context.MonitoringPosts.Remove(monitoringPost);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Monitoring post deleted successfully: {Id}", id);
                }
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in MonitoringPostsController.DeleteConfirmed for id: {Id}", id);
                TempData["ErrorMessage"] = "Ошибка при удалении поста";
                return RedirectToAction(nameof(Index));
            }
        }

        private bool MonitoringPostExists(int id)
        {
            return _context.MonitoringPosts.Any(e => e.Id == id);
        }
    }
}