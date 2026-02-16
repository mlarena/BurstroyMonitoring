using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BurstroyMonitoring.Data;
using BurstroyMonitoring.Data.Models;

namespace BurstroyMonitoring.TCM.Controllers
{
    public class MonitoringPostsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MonitoringPostsController(ApplicationDbContext context)
        {
            _context = context;
        }

       // GET: MonitoringPosts
        public async Task<IActionResult> Index(
            string sortBy = "Id",
            bool sortDesc = true,
            string search = "")
        {
            var query = _context.MonitoringPosts.AsQueryable();

            // Применяем поиск, если есть
            if (!string.IsNullOrEmpty(search))
            {
                search = search.ToLower();
                query = query.Where(mp => 
                    mp.Name.ToLower().Contains(search) ||
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

        // GET: MonitoringPosts/Details/5
        public async Task<IActionResult> Details(int? id)
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

        // GET: MonitoringPosts/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: MonitoringPosts/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Description,Longitude,Latitude,IsMobile,IsActive")] MonitoringPost monitoringPost)
        {
            Console.WriteLine($"=== !СОЗДАНИЕ ПОСТА ===");
            Console.WriteLine($"Name: {monitoringPost.Name}");
            Console.WriteLine($"Longitude: {monitoringPost.Longitude}");
            Console.WriteLine($"Latitude: {monitoringPost.Latitude}");
            Console.WriteLine($"Longitude type: {monitoringPost.Longitude?.GetType()}");
            Console.WriteLine($"Latitude type: {monitoringPost.Latitude?.GetType()}");
            Console.WriteLine($"ModelState valid: {ModelState.IsValid}");
            
            // Вывести все ошибки ModelState
            foreach (var error in ModelState)
            {
                if (error.Value.Errors.Count > 0)
                {
                    Console.WriteLine($"Ошибка в поле {error.Key}: {string.Join(", ", error.Value.Errors.Select(e => e.ErrorMessage))}");
                }
            }
            
            if (ModelState.IsValid)
            {
                monitoringPost.CreatedAt = DateTime.UtcNow;
                monitoringPost.UpdatedAt = DateTime.UtcNow;
                _context.Add(monitoringPost);
                await _context.SaveChangesAsync();
                Console.WriteLine("=== УСПЕШНО СОХРАНЕНО ===");
                return RedirectToAction(nameof(Index));
            }
            
            Console.WriteLine("=== !ОШИБКИ ВАЛИДАЦИИ ===");
            return View(monitoringPost);
        }

        // GET: MonitoringPosts/Edit/5
        public async Task<IActionResult> Edit(int? id)
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

        // POST: MonitoringPosts/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,Longitude,Latitude,IsMobile,IsActive,CreatedAt")] MonitoringPost monitoringPost)
        {
            if (id != monitoringPost.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Преобразуем CreatedAt в UTC, если оно не в UTC
                    if (monitoringPost.CreatedAt.HasValue && monitoringPost.CreatedAt.Value.Kind != DateTimeKind.Utc)
                    {
                        monitoringPost.CreatedAt = monitoringPost.CreatedAt.Value.ToUniversalTime();
                    }
                    
                    monitoringPost.UpdatedAt = DateTime.UtcNow;
                    _context.Update(monitoringPost);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MonitoringPostExists(monitoringPost.Id))
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
            return View(monitoringPost);
        }

        // GET: MonitoringPosts/Delete/5
        public async Task<IActionResult> Delete(int? id)
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

        // POST: MonitoringPosts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var monitoringPost = await _context.MonitoringPosts.FindAsync(id);
            if (monitoringPost != null)
            {
                _context.MonitoringPosts.Remove(monitoringPost);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool MonitoringPostExists(int id)
        {
            return _context.MonitoringPosts.Any(e => e.Id == id);
        }
    }
}