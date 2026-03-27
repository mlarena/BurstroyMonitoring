using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using BurstroyMonitoring.Data;
using BurstroyMonitoring.Data.Models.ViewModels;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace BurstroyMonitoring.TCM.Controllers
{
    public class ReportController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReportController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var viewModel = new SensorSelectionViewModel();
            
            var monitoringPosts = await _context.MonitoringPosts
                .Where(mp => mp.IsActive)
                .OrderBy(mp => mp.Name)
                .ToListAsync();

            viewModel.MonitoringPosts = monitoringPosts
                .Select(mp => new SelectListItem
                {
                    Value = mp.Id.ToString(),
                    Text = mp.Name
                })
                .ToList();

            viewModel.MonitoringPosts.Insert(0, new SelectListItem
            {
                Value = "",
                Text = "Выберите пост мониторинга"
            });

            viewModel.Sensors = new List<SelectListItem>
            {
                new SelectListItem
                {
                    Value = "",
                    Text = "Сначала выберите пост мониторинга"
                }
            };

            return View(viewModel);
        }
    }
}
