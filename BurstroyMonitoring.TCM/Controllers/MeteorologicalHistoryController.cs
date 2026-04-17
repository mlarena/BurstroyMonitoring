using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BurstroyMonitoring.Data;
using System.Linq;
using System.Threading.Tasks;

namespace BurstroyMonitoring.TCM.Controllers
{
    public class MeteorologicalHistoryController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<MeteorologicalHistoryController> _logger;

        public MeteorologicalHistoryController(ApplicationDbContext context, ILogger<MeteorologicalHistoryController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in MeteorologicalHistoryController.Index");
                return View();
            }
        }
    }
}