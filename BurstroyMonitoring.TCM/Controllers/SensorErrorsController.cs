using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BurstroyMonitoring.Data;
using BurstroyMonitoring.Data.Models;
using System.Linq.Dynamic.Core;

namespace BurstroyMonitoring.TCM.Controllers
{
    public class SensorErrorsController : BaseViewController<VwSensorErrorsFull>
    {
        public SensorErrorsController(ApplicationDbContext context) : base(context) { }

        protected override DbSet<VwSensorErrorsFull> DbSet => _context.VwSensorErrorsFull;

        protected override IQueryable<VwSensorErrorsFull> ApplySearch(IQueryable<VwSensorErrorsFull> query, string search)
        {
            search = search.ToLower();
            return query.Where(e =>
                (e.SerialNumber != null && e.SerialNumber.ToLower().Contains(search)) ||
                (e.EndpointName != null && e.EndpointName.ToLower().Contains(search)) ||
                (e.PostName != null && e.PostName.ToLower().Contains(search)) ||
                (e.SensorTypeName != null && e.SensorTypeName.ToLower().Contains(search)) ||
                (e.ErrorMessage != null && e.ErrorMessage.ToLower().Contains(search)) ||
                (e.ExceptionType != null && e.ExceptionType.ToLower().Contains(search)));
        }

        protected override IQueryable<VwSensorErrorsFull> ApplyDefaultSort(IQueryable<VwSensorErrorsFull> query)
        {
            return query.OrderByDescending(e => e.ErrorId);
        }

        // GET: SensorErrors/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var sensorError = await DbSet
                .FirstOrDefaultAsync(m => m.ErrorId == id);

            if (sensorError == null)
            {
                return NotFound();
            }

            return View(sensorError);
        }
    }
}