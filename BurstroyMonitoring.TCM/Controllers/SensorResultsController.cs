using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BurstroyMonitoring.Data;
using BurstroyMonitoring.Data.Models;
using System.Linq.Dynamic.Core;

namespace BurstroyMonitoring.TCM.Controllers
{
    public class SensorResultsController : BaseViewController<VwSensorResultsFull>
    {
        public SensorResultsController(ApplicationDbContext context) : base(context) { }

        protected override DbSet<VwSensorResultsFull> DbSet => _context.VwSensorResultsFull;

        protected override IQueryable<VwSensorResultsFull> ApplySearch(IQueryable<VwSensorResultsFull> query, string search)
        {
            search = search.ToLower();
            return query.Where(e =>
                (e.SerialNumber != null && e.SerialNumber.ToLower().Contains(search)) ||
                (e.EndpointName != null && e.EndpointName.ToLower().Contains(search)) ||
                (e.PostName != null && e.PostName.ToLower().Contains(search)) ||
                (e.SensorTypeName != null && e.SensorTypeName.ToLower().Contains(search)) ||
                (e.ResponseBody != null && e.ResponseBody.ToLower().Contains(search)));
        }

        protected override IQueryable<VwSensorResultsFull> ApplyDefaultSort(IQueryable<VwSensorResultsFull> query)
        {
            return query.OrderByDescending(e => e.ResultId);
        }

        // GET: SensorResults/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var sensorResult = await DbSet
                .FirstOrDefaultAsync(m => m.ResultId == id);

            if (sensorResult == null)
            {
                return NotFound();
            }

            return View(sensorResult);
        }
    }
}