using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BurstroyMonitoring.Data;
using BurstroyMonitoring.Data.Models;
using BurstroyMonitoring.TCM.Models;

namespace BurstroyMonitoring.TCM.Controllers
{
    public class PuidDataController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<PuidDataController> _logger;

        public PuidDataController(ApplicationDbContext context, ILogger<PuidDataController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IActionResult> Index(int? puidId, DateTime? fromDate, DateTime? toDate, int page = 1)
        {
            int pageSize = 3;
            var availablePuids = await _context.Puids
                .OrderBy(p => p.EndPointsName)
                .ToListAsync();

            if (!puidId.HasValue && availablePuids.Any())
            {
                puidId = availablePuids.First().Id;
            }

            var query = _context.PuidData.AsQueryable();
            if (puidId.HasValue)
            {
                query = query.Where(d => d.PuidId == puidId.Value);
            }

            if (fromDate.HasValue)
            {
                var from = DateTime.SpecifyKind(fromDate.Value, DateTimeKind.Utc);
                query = query.Where(d => d.RangeStart >= from);
            }

            if (toDate.HasValue)
            {
                var to = DateTime.SpecifyKind(toDate.Value, DateTimeKind.Utc);
                query = query.Where(d => d.RangeEnd <= to);
            }

            // Для корректной пагинации сгруппированных данных сначала получим уникальные ключи групп
            var groupKeysQuery = query
                .GroupBy(d => new { d.RangeStart, d.RangeEnd, d.MessageId })
                .Select(g => new { g.Key.RangeStart, g.Key.RangeEnd, g.Key.MessageId })
                .OrderByDescending(k => k.RangeStart);

            int totalGroups = await groupKeysQuery.CountAsync();
            var pagedKeys = await groupKeysQuery
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Теперь загружаем все данные для этих конкретных групп
            var pagedMessageIds = pagedKeys.Select(k => k.MessageId).ToList();
            var data = await query
                .Where(d => pagedMessageIds.Contains(d.MessageId))
                .OrderByDescending(d => d.RangeStart)
                .ThenBy(d => d.Lane)
                .ToListAsync();

            var groupedData = data
                .GroupBy(d => new { d.RangeStart, d.RangeEnd, d.MessageId })
                .Select(g => new PuidDataGroup
                {
                    RangeStart = g.Key.RangeStart,
                    RangeEnd = g.Key.RangeEnd,
                    MessageId = g.Key.MessageId,
                    Lanes = g.OrderBy(l => l.Lane).ToList()
                })
                .OrderByDescending(g => g.RangeStart)
                .ToList();

            var viewModel = new PuidViewModel
            {
                GroupedItems = groupedData,
                AvailablePuids = availablePuids,
                SelectedPuidId = puidId,
                CurrentEndPointName = availablePuids.FirstOrDefault(p => p.Id == puidId)?.EndPointsName,
                CurrentSensorName = data.FirstOrDefault()?.SensorName,
                FromDate = fromDate,
                ToDate = toDate,
                PageNumber = page,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling(totalGroups / (double)pageSize),
                TotalStats = CalculateTotalStats(data),
                AverageStats = CalculateAverageStats(data)
            };

            return View(viewModel);
        }

        private TrafficStatistics CalculateTotalStats(List<PuidData> data)
        {
            if (!data.Any()) return new TrafficStatistics();

            return new TrafficStatistics
            {
                TotalVolume = data.Sum(d => d.Volume),
                Class0 = data.Sum(d => d.Class0),
                Class1 = data.Sum(d => d.Class1),
                Class2 = data.Sum(d => d.Class2),
                Class3 = data.Sum(d => d.Class3),
                Class4 = data.Sum(d => d.Class4),
                Class5 = data.Sum(d => d.Class5),
                AvgSpeed = data.Any() ? data.Average(d => d.SpeedAvg) : 0,
                AvgOccupancy = data.Any() ? data.Average(d => d.OccupancyPrc) : 0
            };
        }

        private TrafficStatistics CalculateAverageStats(List<PuidData> data)
        {
            if (!data.Any()) return new TrafficStatistics();

            return new TrafficStatistics
            {
                TotalVolume = (long)data.Average(d => d.Volume),
                Class0 = (long)data.Average(d => d.Class0),
                Class1 = (long)data.Average(d => d.Class1),
                Class2 = (long)data.Average(d => d.Class2),
                Class3 = (long)data.Average(d => d.Class3),
                Class4 = (long)data.Average(d => d.Class4),
                Class5 = (long)data.Average(d => d.Class5),
                AvgSpeed = data.Average(d => d.SpeedAvg),
                AvgOccupancy = data.Average(d => d.OccupancyPrc)
            };
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var data = await _context.PuidData
                .FirstOrDefaultAsync(m => m.Id == id);

            if (data == null) return NotFound();

            return View(data);
        }
    }
}