using Microsoft.AspNetCore.Mvc;

namespace BurstroyMonitoring.TCM.Controllers
{
    public class GraphsAndChartsController : Controller
    {
        public IActionResult Index()
        {
            ViewData["Title"] = "Графики и диаграммы";
            return View();
        }
    }
}