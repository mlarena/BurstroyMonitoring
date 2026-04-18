using Microsoft.AspNetCore.Mvc;

namespace BurstroyMonitoring.TCM.Controllers
{
    public class AboutController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
