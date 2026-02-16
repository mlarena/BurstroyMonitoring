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

        // GET: Index с пагинацией и фильтрацией
        public virtual async Task<IActionResult> Index(
            int page = 1,
            int pageSize = 10,
            string search = "",
            string sortBy = "",
            bool sortDesc = false)
        {
            var query = DbSet.AsQueryable();

            // Применяем поиск
            if (!string.IsNullOrEmpty(search))
            {
                query = ApplySearch(query, search);
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

            // Передаем данные в View
            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalCount = totalCount;
            ViewBag.Search = search;
            ViewBag.SortBy = sortBy;
            ViewBag.SortDesc = sortDesc;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            return View(items);
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
                .AsEnumerable(); // Переключаемся на Linq to Objects для безопасного использования GetValue

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