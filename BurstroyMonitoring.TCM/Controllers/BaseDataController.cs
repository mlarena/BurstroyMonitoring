using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BurstroyMonitoring.Data;
using BurstroyMonitoring.Data.Models.ViewModels;
using BurstroyMonitoring.TCM.Services;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BurstroyMonitoring.TCM.Controllers
{
    public abstract class BaseDataController<T> : BaseViewController<T> where T : class
    {
        private readonly IExportService _exportService;

        protected BaseDataController(ApplicationDbContext context, IExportService exportService) : base(context)
        {
            _exportService = exportService;
        }

        protected abstract string FilterSessionKey { get; }
        protected abstract Dictionary<string, string> GetRussianColumnNames();

        // GET: Details/5 (общий для всех)
        public virtual async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var data = await DbSet.FirstOrDefaultAsync(m => EF.Property<int>(m, $"{typeof(T).Name.Replace("Vw", "").Replace("Full", "")}Id") == id);
            if (data == null) return NotFound();

            return View(data);
        }

        // DataViewer (общая логика)
        public async Task<IActionResult> DataViewer(DataFilterViewModel filter)
        {
            // Логируем входные параметры
            Console.WriteLine($"[DEBUG] DataViewer вызван. SelectedFields count: {filter.SelectedFields?.Count ?? 0}");

            // Обработка сессии для фильтра (идентичная логика)
            if (HttpContext.Session.TryGetValue(FilterSessionKey, out byte[] sessionData))
            {
                var savedFilter = DeserializeFromSession(sessionData);

                if (IsFilterEmpty(filter))
                {
                    Console.WriteLine($"[DEBUG] Используем сохраненный фильтр из сессии");
                    filter = savedFilter;
                }
                else
                {
                    if (filter.SelectedFields == null || !filter.SelectedFields.Any())
                    {
                        filter.SelectedFields = savedFilter.SelectedFields;
                        Console.WriteLine($"[DEBUG] Восстановили SelectedFields из сессии: {filter.SelectedFields?.Count ?? 0} полей");
                    }
                    SaveFilterToSession(filter);
                }
            }
            else
            {
                if (!IsFilterEmpty(filter))
                {
                    SaveFilterToSession(filter);
                    Console.WriteLine($"[DEBUG] Сохранили новый фильтр в сессию");
                }
            }

            var query = DbSet.AsQueryable();

            // Уникальные значения для списков (общее)
           var serialNumbers = await DbSet
                .Where(x => EF.Property<string>(x, "SerialNumber") != null &&
                            !string.IsNullOrEmpty(EF.Property<string>(x, "SerialNumber")))
                .Select(x => EF.Property<string>(x, "SerialNumber")!)
                .Distinct()
                .OrderBy(x => x)
                .ToListAsync();

            var endpointNames = await DbSet
                .Where(x => EF.Property<string>(x, "EndpointName") != null &&
                            !string.IsNullOrEmpty(EF.Property<string>(x, "EndpointName")))
                .Select(x => EF.Property<string>(x, "EndpointName")!)
                .Distinct()
                .OrderBy(x => x)
                .ToListAsync();

            // Применение фильтров (общее, с исправлением на Contains для SerialNumber)
            if (filter.StartDate.HasValue)
            {
                var startDate = filter.StartDate.Value.Kind == DateTimeKind.Unspecified 
                    ? DateTime.SpecifyKind(filter.StartDate.Value, DateTimeKind.Utc) 
                    : filter.StartDate.Value;
                query = query.Where(x => EF.Property<DateTime>(x, "ReceivedAt") >= startDate);
            }

            if (filter.EndDate.HasValue)
            {
                var endDate = filter.EndDate.Value.Kind == DateTimeKind.Unspecified 
                    ? DateTime.SpecifyKind(filter.EndDate.Value, DateTimeKind.Utc) 
                    : filter.EndDate.Value;
                query = query.Where(x => EF.Property<DateTime>(x, "ReceivedAt") <= endDate);
            }

            if (filter.SensorIsActive.HasValue)
                query = query.Where(x => EF.Property<bool>(x, "SensorIsActive") == filter.SensorIsActive.Value);

            if (filter.PostIsActive.HasValue)
                query = query.Where(x => EF.Property<bool>(x, "PostIsActive") == filter.PostIsActive.Value);

            if (!string.IsNullOrEmpty(filter.SerialNumber))
                query = query.Where(x => EF.Property<string>(x, "SerialNumber")!.Contains(filter.SerialNumber));

            if (!string.IsNullOrEmpty(filter.EndpointName))
                query = query.Where(x => EF.Property<string>(x, "EndpointName") == filter.EndpointName);

            int totalCount = await query.CountAsync();

            // Пагинация (общая)
            List<T> resultData;
            switch (filter.PageSize?.ToLower())
            {
                case "100":
                    resultData = await query.OrderByDescending(x => EF.Property<DateTime>(x, "ReceivedAt")).Take(100).ToListAsync();
                    break;
                case "всё":
                case "all":
                    resultData = await query.OrderByDescending(x => EF.Property<DateTime>(x, "ReceivedAt")).ToListAsync();
                    break;
                case "10":
                default:
                    resultData = await query.OrderByDescending(x => EF.Property<DateTime>(x, "ReceivedAt")).Take(10).ToListAsync();
                    filter.PageSize = "10";
                    break;
            }

            // Выбор полей по умолчанию
            if (filter.SelectedFields == null || !filter.SelectedFields.Any())
            {
                var allFields = GetAvailableFields<T>();
                filter.SelectedFields = allFields.Take(5).ToList();
                SaveFilterToSession(filter);
            }

            ViewBag.Filter = filter;
            ViewBag.AvailableFields = GetAvailableFields<T>();
            ViewBag.SerialNumbers = serialNumbers;
            ViewBag.EndpointNames = endpointNames;
            ViewBag.TotalCount = totalCount;

            return View(resultData);
        }

        // Export (общая логика)
        [HttpPost]
        public async Task<IActionResult> Export(DataFilterViewModel filter, string format, bool exportAllData = false)
        {
            try
            {
                Console.WriteLine($"[DEBUG] Начало экспорта. Формат: {format}, ExportAllData: {exportAllData}");

                // Восстановление SelectedFields из сессии
                if (filter.SelectedFields == null || !filter.SelectedFields.Any())
                {
                    if (HttpContext.Session.TryGetValue(FilterSessionKey, out byte[] sessionData))
                    {
                        var savedFilter = DeserializeFromSession(sessionData);
                        filter.SelectedFields = savedFilter.SelectedFields;
                        Console.WriteLine($"[DEBUG] Восстановили поля для экспорта из сессии: {filter.SelectedFields?.Count ?? 0}");
                    }
                }

                var query = DbSet.AsQueryable();

                // Применение фильтров (аналогично DataViewer, с Contains для SerialNumber)
                if (filter.StartDate.HasValue)
                {
                    var startDate = filter.StartDate.Value.Kind == DateTimeKind.Unspecified 
                        ? DateTime.SpecifyKind(filter.StartDate.Value, DateTimeKind.Utc) 
                        : filter.StartDate.Value;
                    query = query.Where(x => EF.Property<DateTime>(x, "ReceivedAt") >= startDate);
                }

                if (filter.EndDate.HasValue)
                {
                    var endDate = filter.EndDate.Value.Kind == DateTimeKind.Unspecified 
                        ? DateTime.SpecifyKind(filter.EndDate.Value, DateTimeKind.Utc) 
                        : filter.EndDate.Value;
                    query = query.Where(x => EF.Property<DateTime>(x, "ReceivedAt") <= endDate);
                }

                if (filter.SensorIsActive.HasValue)
                    query = query.Where(x => EF.Property<bool>(x, "SensorIsActive") == filter.SensorIsActive.Value);

                if (filter.PostIsActive.HasValue)
                    query = query.Where(x => EF.Property<bool>(x, "PostIsActive") == filter.PostIsActive.Value);

                if (!string.IsNullOrEmpty(filter.SerialNumber))
                    query = query.Where(x => EF.Property<string>(x, "SerialNumber")!.Contains(filter.SerialNumber));

                if (!string.IsNullOrEmpty(filter.EndpointName))
                    query = query.Where(x => EF.Property<string>(x, "EndpointName") == filter.EndpointName);

                query = query.OrderByDescending(x => EF.Property<DateTime>(x, "ReceivedAt"));

                int totalRecords = await query.CountAsync();
                List<T> data;

                if (exportAllData || filter.PageSize?.ToLower() is "all" or "всё")
                {
                    data = await query.ToListAsync();
                    Console.WriteLine($"[DEBUG] Экспорт ВСЕХ данных с фильтрами: {data.Count} из {totalRecords} записей");
                }
                else
                {
                    int take = filter.PageSize?.ToLower() switch
                    {
                        "100" => 100,
                        _ => 10
                    };
                    data = await query.Take(take).ToListAsync();
                    Console.WriteLine($"[DEBUG] Экспорт первых {take} записей: {data.Count} из {totalRecords}");
                }

                if (filter.SelectedFields == null || !filter.SelectedFields.Any())
                {
                    filter.SelectedFields = GetAvailableFields<T>();
                    Console.WriteLine($"[DEBUG] Используем все поля: {filter.SelectedFields.Count}");
                }
                else
                {
                    Console.WriteLine($"[DEBUG] Выбранные поля: {string.Join(", ", filter.SelectedFields)}");
                }

                // Подготовка полей и названий (используем абстрактный метод)
                var russianColumnNames = GetRussianColumnNames();
                var exportFields = filter.SelectedFields;
                var exportDisplayNames = exportFields.Select(field => 
                    russianColumnNames.TryGetValue(field, out var name) ? name : field).ToList();

                byte[] fileContents;
                string contentType;
                string fileName = $"{typeof(T).Name.ToLower().Replace("vw", "").Replace("full", "")}_data_{DateTime.Now:yyyyMMdd_HHmmss}";

                if (format == "excel")
                {
                    Console.WriteLine($"[DEBUG] Создание Excel файла...");
                    fileContents = _exportService.ExportToExcel(data, exportFields, exportDisplayNames);
                    contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                    fileName += ".xlsx";
                }
                else
                {
                    Console.WriteLine($"[DEBUG] Создание CSV файла...");
                    fileContents = _exportService.ExportToCsv(data, exportFields, exportDisplayNames);
                    contentType = "text/csv; charset=utf-8";
                    fileName += ".csv";
                }

                Console.WriteLine($"[DEBUG] Экспорт завершен успешно ({fileContents.Length} байт)");
                return File(fileContents, contentType, fileName);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Ошибка экспорта: {ex.Message}\n[ERROR] StackTrace: {ex.StackTrace}");
                TempData["ErrorMessage"] = $"Ошибка при экспорте: {ex.Message}";
                return RedirectToAction("DataViewer", filter);
            }
        }

        // Сброс фильтров
        public IActionResult Reset()
        {
            HttpContext.Session.Remove(FilterSessionKey);
            Console.WriteLine($"[DEBUG] Фильтры сброшены");
            TempData["Message"] = "Фильтры сброшены";
            return RedirectToAction("DataViewer");
        }

        // Сохранение фильтра (AJAX)
        [HttpPost]
        public IActionResult SaveFilter([FromBody] DataFilterViewModel filter)
        {
            SaveFilterToSession(filter);
            Console.WriteLine($"[DEBUG] Фильтр сохранен вручную");
            return Json(new { success = true });
        }

        // Вспомогательные методы (общие)
        private void SaveFilterToSession(DataFilterViewModel filter)
        {
            var options = new JsonSerializerOptions { ReferenceHandler = ReferenceHandler.Preserve, WriteIndented = false };
            var json = JsonSerializer.Serialize(filter, options);
            var bytes = System.Text.Encoding.UTF8.GetBytes(json);
            HttpContext.Session.Set(FilterSessionKey, bytes);
        }

        private DataFilterViewModel DeserializeFromSession(byte[] sessionData)
        {
            var json = System.Text.Encoding.UTF8.GetString(sessionData);
            var options = new JsonSerializerOptions { ReferenceHandler = ReferenceHandler.Preserve, PropertyNameCaseInsensitive = true };
            return JsonSerializer.Deserialize<DataFilterViewModel>(json, options) ?? new DataFilterViewModel();
        }

        private bool IsFilterEmpty(DataFilterViewModel filter)
        {
            return !filter.StartDate.HasValue &&
                   !filter.EndDate.HasValue &&
                   string.IsNullOrEmpty(filter.SerialNumber) &&
                   string.IsNullOrEmpty(filter.EndpointName) &&
                   !filter.SensorIsActive.HasValue &&
                   !filter.PostIsActive.HasValue &&
                   (!filter.SelectedFields?.Any() ?? true);
        }

        private List<string> GetAvailableFields<U>()
        {
            return typeof(U).GetProperties().Select(p => p.Name).ToList();
        }
    }
}