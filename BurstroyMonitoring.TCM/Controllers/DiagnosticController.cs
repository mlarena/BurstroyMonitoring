using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BurstroyMonitoring.TCM.Controllers
{
    [Authorize]
    public class DiagnosticController : Controller
    {
        private readonly ILogger<DiagnosticController> _logger;

        public DiagnosticController(ILogger<DiagnosticController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public IActionResult TestLogging()
        {
            _logger.LogInformation("DiagnosticController.TestLogging called");
            return Content("Check console logs for diagnostic messages");
        }

        [HttpGet]
        public IActionResult TestWithId(int id)
        {
            _logger.LogInformation("DiagnosticController.TestWithId called with id={Id}", id);
            return Content($"ID received: {id}");
        }
    }
}