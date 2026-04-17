using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BurstroyMonitoring.Data;
using BurstroyMonitoring.Data.Models;
using BurstroyMonitoring.TCM.Services;

namespace BurstroyMonitoring.TCM.Controllers
{
    public class CamerasController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly RtspStreamService _streamService;
        private readonly ILogger<CamerasController> _logger;

        public CamerasController(ApplicationDbContext context, RtspStreamService streamService, ILogger<CamerasController> logger)
        {
            _context = context;
            _streamService = streamService;
            _logger = logger;
        }

        // Список камер
        public async Task<IActionResult> Index()
        {
            try
            {
                var cameras = await _context.Cameras
                    .Include(c => c.MonitoringPost)
                    .ToListAsync();

                // Получаем последние скриншоты для каждой камеры
                var snapshots = await _context.Snapshots
                    .OrderByDescending(s => s.CreatedAt)
                    .ToListAsync();

                var lastSnapshots = snapshots
                    .GroupBy(s => s.CameraId)
                    .ToDictionary(g => g.Key, g => g.First().FilePath);

                ViewBag.LastSnapshots = lastSnapshots;
                
                // Также посчитаем общее количество снимков для каждой камеры
                var snapshotCounts = snapshots
                    .GroupBy(s => s.CameraId)
                    .ToDictionary(g => g.Key, g => g.Count());
                
                ViewBag.SnapshotCounts = snapshotCounts;

                return View(cameras);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in CamerasController.Index");
                return View(new List<Camera>());
            }
        }

        // Просмотр конкретной камеры и управление
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var camera = await _context.Cameras
                    .Include(c => c.MonitoringPost)
                    .FirstOrDefaultAsync(m => m.Id == id);
                
                if (camera == null) return NotFound();

                var snapshots = await _context.Snapshots
                    .Where(s => s.CameraId == id)
                    .OrderByDescending(s => s.CreatedAt)
                    .Take(6)
                    .ToListAsync();

                ViewBag.LastSnapshots = snapshots;
                ViewBag.LatestSnapshotPath = snapshots.FirstOrDefault()?.FilePath;
                
                return View(camera);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in CamerasController.Details for id: {Id}", id);
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> TakeSnapshot(int id)
        {
            try
            {
                var camera = await _context.Cameras.FindAsync(id);
                if (camera == null) return Json(new { success = false, error = "Камера не найдена" });

                var frame = _streamService.GetFrame();
                if (frame == null) return Json(new { success = false, error = "Не удалось получить кадр из потока" });

                var fileName = $"{Guid.NewGuid()}.jpg";
                var relativePath = Path.Combine("snapshots", id.ToString(), fileName);
                var absolutePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", relativePath);

                Directory.CreateDirectory(Path.GetDirectoryName(absolutePath)!);
                await System.IO.File.WriteAllBytesAsync(absolutePath, frame);

                var snapshot = new Snapshot
                {
                    CameraId = id,
                    FilePath = "/" + relativePath.Replace("\\", "/"),
                    CreatedAt = DateTime.UtcNow
                };

                _context.Snapshots.Add(snapshot);
                await _context.SaveChangesAsync();

                return Json(new { success = true, filePath = snapshot.FilePath, createdAt = snapshot.CreatedAt.ToString("HH:mm:ss") });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in TakeSnapshot for camera id: {Id}", id);
                return Json(new { success = false, error = ex.Message });
            }
        }

        public async Task<IActionResult> Snapshots(int? cameraId, DateTime? from, DateTime? to, int page = 1)
        {
            try
            {
                int pageSize = 15;
                var query = _context.Snapshots.Include(s => s.Camera).AsQueryable();

                if (cameraId.HasValue)
                    query = query.Where(s => s.CameraId == cameraId);
                
                if (from.HasValue)
                    query = query.Where(s => s.CreatedAt >= from.Value.ToUniversalTime());
                
                if (to.HasValue)
                    query = query.Where(s => s.CreatedAt <= to.Value.ToUniversalTime());

                var totalItems = await query.CountAsync();
                var snapshots = await query
                    .OrderByDescending(s => s.CreatedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                ViewBag.Cameras = new SelectList(await _context.Cameras.ToListAsync(), "Id", "Name", cameraId);
                ViewBag.CurrentPage = page;
                ViewBag.TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
                ViewBag.CameraId = cameraId;
                ViewBag.From = from;
                ViewBag.To = to;
                
                return View(snapshots);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in CamerasController.Snapshots");
                return View(new List<Snapshot>());
            }
        }
        // Создание/подключение новой камеры
        public async Task<IActionResult> Create()
        {
            try
            {
                ViewBag.MonitoringPostId = new SelectList(await _context.MonitoringPosts.ToListAsync(), "Id", "Name");
                return View(new Camera());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in CamerasController.Create (GET)");
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Camera camera)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    _context.Add(camera);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                ViewBag.MonitoringPostId = new SelectList(await _context.MonitoringPosts.ToListAsync(), "Id", "Name", camera.MonitoringPostId);
                return View(camera);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in CamerasController.Create (POST)");
                ModelState.AddModelError("", "Ошибка при создании камеры");
                ViewBag.MonitoringPostId = new SelectList(await _context.MonitoringPosts.ToListAsync(), "Id", "Name", camera.MonitoringPostId);
                return View(camera);
            }
        }

        // Редактирование настроек камеры
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var camera = await _context.Cameras.FindAsync(id);
                if (camera == null) return NotFound();
                ViewBag.MonitoringPostId = new SelectList(await _context.MonitoringPosts.ToListAsync(), "Id", "Name", camera.MonitoringPostId);
                return View(camera);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in CamerasController.Edit (GET) for id: {Id}", id);
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Camera camera)
        {
            if (id != camera.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    // Убеждаемся, что дата имеет Kind = Utc для PostgreSQL
                    if (camera.CreatedAt.Kind == DateTimeKind.Unspecified)
                    {
                        camera.CreatedAt = DateTime.SpecifyKind(camera.CreatedAt, DateTimeKind.Utc);
                    }
                    else if (camera.CreatedAt.Kind == DateTimeKind.Local)
                    {
                        camera.CreatedAt = camera.CreatedAt.ToUniversalTime();
                    }

                    _context.Update(camera);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    if (!CameraExists(camera.Id)) return NotFound();
                    else
                    {
                        _logger.LogError(ex, "Concurrency error in CamerasController.Edit for id: {Id}", id);
                        throw;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in CamerasController.Edit (POST) for id: {Id}", id);
                    ModelState.AddModelError("", "Ошибка при сохранении изменений");
                }
            }
            ViewBag.MonitoringPostId = new SelectList(await _context.MonitoringPosts.ToListAsync(), "Id", "Name", camera.MonitoringPostId);
            return View(camera);
        }

        // Удаление камеры
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var camera = await _context.Cameras
                    .Include(c => c.MonitoringPost)
                    .FirstOrDefaultAsync(m => m.Id == id);
                
                if (camera == null) return NotFound();
                return View(camera);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in CamerasController.Delete (GET) for id: {Id}", id);
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var camera = await _context.Cameras.FindAsync(id);
                if (camera != null)
                {
                    _context.Cameras.Remove(camera);
                    await _context.SaveChangesAsync();
                }
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in CamerasController.DeleteConfirmed for id: {Id}", id);
                TempData["ErrorMessage"] = "Ошибка при удалении камеры";
                return RedirectToAction(nameof(Index));
            }
        }

        private bool CameraExists(int id)
        {
            return _context.Cameras.Any(e => e.Id == id);
        }

        // API для управления потоком
        [HttpPost]
        [IgnoreAntiforgeryToken]
        public IActionResult StartStream(int id)
        {
            try
            {
                var camera = _context.Cameras.Find(id);
                if (camera == null) return Json(new { success = false, error = "Камера не найдена" });

                _streamService.Dispose(); // Останавливаем предыдущий поток
                
                string fullUrl = camera.RtspUrl;
                if (!string.IsNullOrEmpty(camera.Username) && !camera.RtspUrl.Contains("@"))
                {
                    fullUrl = camera.RtspUrl.Replace("://", $"://{camera.Username}:{camera.Password}@");
                }

                _streamService.Start(fullUrl);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in StartStream for camera id: {Id}", id);
                return Json(new { success = false, error = ex.Message });
            }
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public IActionResult StopStream()
        {
            _streamService.Dispose();
            return Json(new { success = true });
        }

        [HttpGet]
        public IActionResult GetFrame()
        {
            var frame = _streamService.GetFrame();
            if (frame == null) return NotFound();
            return File(frame, "image/jpeg");
        }
    }
}
