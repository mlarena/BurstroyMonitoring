using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BurstroyMonitoring.Data;
using BurstroyMonitoring.TCM.Attributes;

namespace BurstroyMonitoring.TCM.Controllers;

[Authorize]
[SkipLogging("Веб-части: частые GET-запросы не нужно логировать")]
public class WebPartsController : Controller
{
    private readonly ApplicationDbContext _context;

    public WebPartsController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> GetMonitoringPostsWebPart()
    {
        var posts = await _context.MonitoringPosts
            .Include(p => p.Sensors)
            .OrderBy(p => p.Name)
            .ToListAsync();
        return PartialView("~/Views/Shared/WebParts/_MonitoringPosts.cshtml", posts);
    }

    public IActionResult GetMonitoringMapWebPart()
    {
        return PartialView("~/Views/Shared/WebParts/_MonitoringMap.cshtml");
    }

    public async Task<IActionResult> GetSensorDataWebPart()
    {
        var sensors = await _context.Sensors
            .Include(s => s.SensorType)
            .Include(s => s.MonitoringPost)
            .Where(s => s.IsActive)
            .OrderBy(s => s.MonitoringPost!.Name)
            .ThenBy(s => s.EndPointsName)
            .ToListAsync();
        return PartialView("~/Views/Shared/WebParts/_SensorData.cshtml", sensors);
    }

    public IActionResult GetGraphsAndChartsWebPart()
    {
        return PartialView("~/Views/Shared/WebParts/_GraphsAndCharts.cshtml");
    }

    public IActionResult GetReportWebPart()
    {
        return PartialView("~/Views/Shared/WebParts/_Report.cshtml");
    }

    public async Task<IActionResult> GetCamerasWebPart()
    {
        var cameras = await _context.Cameras.OrderBy(c => c.Name).ToListAsync();
        return PartialView("~/Views/Shared/WebParts/_Cameras.cshtml", cameras);
    }
}
