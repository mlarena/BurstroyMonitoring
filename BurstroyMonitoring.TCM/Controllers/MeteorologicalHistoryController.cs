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

        public MeteorologicalHistoryController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            return View();
        }
    }
}