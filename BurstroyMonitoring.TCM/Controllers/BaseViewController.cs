using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BurstroyMonitoring.Data;
using System.Linq.Dynamic.Core;
using System.Reflection;

namespace BurstroyMonitoring.TCM.Controllers
{
    public abstract class BaseViewController<T> : Controller where T : class
    {
        protected readonly ApplicationDbContext _context;
        protected abstract DbSet<T> DbSet { get; }

        protected BaseViewController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Index с пагинацией, поиском и фильтрацией
        public virtual async Task<IActionResult> Index(
            int page = 1,
            int pageSize = 10,
            string search = "",
            string sortBy = "",
            bool sortDesc = false,
            List<int> selectedSensorTypes = null,
            List<int> selectedMonitoringPosts = null,
            int? sensorTypeId = null,
            int? monitoringPostId = null)
        {
            var query = DbSet.AsQueryable();

            // Применяем поиск
            if (!string.IsNullOrEmpty(search))
            {
                query = ApplySearch(query, search);
            }

            // Применяем фильтрацию по типу датчика
            if (selectedSensorTypes != null && selectedSensorTypes.Any())
            {
                query = ApplySensorTypeFilter(query, selectedSensorTypes);
            }
            else if (sensorTypeId.HasValue)
            {
                query = ApplySensorTypeFilter(query, new List<int> { sensorTypeId.Value });
            }

            // Применяем фильтрацию по посту мониторинга
            if (selectedMonitoringPosts != null && selectedMonitoringPosts.Any())
            {
                query = ApplyMonitoringPostFilter(query, selectedMonitoringPosts);
            }
            else if (monitoringPostId.HasValue)
            {
                query = ApplyMonitoringPostFilter(query, new List<int> { monitoringPostId.Value });
            }

            // Применяем сортировку
            if (!string.IsNullOrEmpty(sortBy))
            {
                var sortDirection = sortDesc ? "descending" : "ascending";
                query = query.OrderBy($"{sortBy} {sortDirection}");
            }
            else
            {
                // Сортировка по умолчанию - по первому полю id в порядке убывания
                query = ApplyDefaultSort(query);
            }

            // Получаем общее количество записей
            var totalCount = await query.CountAsync();

            // Применяем пагинацию
            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Получаем данные для фильтров
            var filterData = await GetFilterData();

            // Передаем данные в View
            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalCount = totalCount;
            ViewBag.Search = search;
            ViewBag.SortBy = sortBy;
            ViewBag.SortDesc = sortDesc;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalCount / pageSize);
            
            // Данные для фильтров
            ViewBag.SensorTypes = filterData.SensorTypes;
            ViewBag.MonitoringPosts = filterData.MonitoringPosts;
            
            // Выбранные фильтры
            ViewBag.SelectedSensorTypeIds = selectedSensorTypes ?? new List<int>();
            ViewBag.SelectedMonitoringPostIds = selectedMonitoringPosts ?? new List<int>();
            ViewBag.SelectedSensorTypeId = sensorTypeId;
            ViewBag.SelectedMonitoringPostId = monitoringPostId;

            return View(items);
        }

        // Метод для получения данных фильтров
        protected virtual async Task<(List<dynamic> SensorTypes, List<dynamic> MonitoringPosts)> GetFilterData()
        {
            // Получаем уникальные типы датчиков из данных
            var sensorTypes = await DbSet
                .AsQueryable()
                .Select(e => new { 
                    SensorTypeId = EF.Property<int?>(e, "SensorTypeId"),
                    SensorTypeName = EF.Property<string>(e, "SensorTypeName")
                })
                .Where(x => x.SensorTypeId.HasValue && x.SensorTypeName != null)
                .Distinct()
                .OrderBy(x => x.SensorTypeName)
                .Select(x => new { x.SensorTypeId, x.SensorTypeName })
                .ToListAsync();

            // Получаем уникальные посты мониторинга из данных
            var monitoringPosts = await DbSet
                .AsQueryable()
                .Select(e => new { 
                    PostId = EF.Property<int?>(e, "PostId"),
                    PostName = EF.Property<string>(e, "PostName")
                })
                .Where(x => x.PostId.HasValue && x.PostName != null)
                .Distinct()
                .OrderBy(x => x.PostName)
                .Select(x => new { PostId = x.PostId, Name = x.PostName })
                .ToListAsync();

            return (sensorTypes.Cast<dynamic>().ToList(), monitoringPosts.Cast<dynamic>().ToList());
        }

        // Метод для фильтрации по типу датчика
        protected virtual IQueryable<T> ApplySensorTypeFilter(IQueryable<T> query, List<int> sensorTypeIds)
        {
            return query.Where(e => sensorTypeIds.Contains(EF.Property<int>(e, "SensorTypeId")));
        }

        // Метод для фильтрации по посту мониторинга
        protected virtual IQueryable<T> ApplyMonitoringPostFilter(IQueryable<T> query, List<int> postIds)
        {
            return query.Where(e => postIds.Contains(EF.Property<int>(e, "PostId")));
        }

        // Метод для автозаполнения
        [HttpGet("autocomplete")]
        public virtual async Task<IActionResult> Autocomplete(string term)
        {
            if (string.IsNullOrWhiteSpace(term) || term.Length < 2)
                return Json(new List<string>());

            term = term.ToLower();

            // Получаем свойства для поиска
            var serialNumberProp = typeof(T).GetProperty("SerialNumber");
            var endpointNameProp = typeof(T).GetProperty("EndpointName");

            if (serialNumberProp == null || endpointNameProp == null)
                return Json(new List<string>());

            var query = DbSet.AsQueryable();
            
            // Фильтруем данные
            var filtered = query
                .Where(e => 
                    (serialNumberProp.GetValue(e) != null && serialNumberProp.GetValue(e).ToString().ToLower().Contains(term)) ||
                    (endpointNameProp.GetValue(e) != null && endpointNameProp.GetValue(e).ToString().ToLower().Contains(term)))
                .AsEnumerable();

            // Получаем предложения
            var suggestions = filtered
                .Select(e => 
                    (serialNumberProp.GetValue(e)?.ToString() ?? "") + 
                    " | " + 
                    (endpointNameProp.GetValue(e)?.ToString() ?? ""))
                .Where(s => !string.IsNullOrEmpty(s))
                .Distinct()
                .Take(10)
                .ToList();

            return Json(suggestions);
        }

        // Абстрактные методы для реализации в дочерних классах
        protected abstract IQueryable<T> ApplySearch(IQueryable<T> query, string search);
        protected abstract IQueryable<T> ApplyDefaultSort(IQueryable<T> query);
    }
}