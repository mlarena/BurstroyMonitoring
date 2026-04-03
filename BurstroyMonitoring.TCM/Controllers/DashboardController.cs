using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BurstroyMonitoring.Data.Models;
using BurstroyMonitoring.Data.Models.ViewModels;
using BurstroyMonitoring.TCM.Services;

namespace BurstroyMonitoring.TCM.Controllers;

[Authorize]
[IgnoreAntiforgeryToken]
public class DashboardController : Controller
{
    private readonly IWebPartService _webPartService;
    private readonly ILogger<DashboardController> _logger;

    public DashboardController(IWebPartService webPartService, ILogger<DashboardController> logger)
    {
        _webPartService = webPartService;
        _logger = logger;
    }

    public async Task<IActionResult> Index()
    {
        var userId = GetCurrentUserId();
        var webParts = await _webPartService.GetUserWebPartsAsync(userId);

        var model = new DashboardViewModel
        {
            WebParts = webParts,
            AvailableWebParts = Enum.GetValues<WebPartType>().ToList()
        };

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> AddWebPart(WebPartType type, string title)
    {
        try
        {
            var userId = GetCurrentUserId();
            var webPart = await _webPartService.AddWebPartAsync(userId, type, title);
            return PartialView("_WebPart", webPart);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при добавлении веб-части");
            return BadRequest("Ошибка при добавлении веб-части");
        }
    }

    [HttpPost]
    public async Task<IActionResult> RemoveWebPart(int webPartId)
    {
        try
        {
            await _webPartService.RemoveWebPartAsync(webPartId);
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при удалении веб-части {Id}", webPartId);
            return BadRequest("Ошибка при удалении");
        }
    }

    [HttpPost]
    public async Task<IActionResult> UpdateWebPartPosition(int webPartId, int x, int y)
    {
        try
        {
            var userId = GetCurrentUserId();
            var webParts = await _webPartService.GetUserWebPartsAsync(userId);
            var webPart = webParts.FirstOrDefault(w => w.Id == webPartId);
            if (webPart != null)
            {
                webPart.PositionX = x;
                webPart.PositionY = y;
                await _webPartService.UpdateWebPartAsync(webPart);
            }
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при обновлении позиции {Id}", webPartId);
            return BadRequest("Ошибка при обновлении позиции");
        }
    }

    [HttpPost]
    public async Task<IActionResult> UpdateWebPartSize(int webPartId, int width, int height)
    {
        try
        {
            var userId = GetCurrentUserId();
            var webParts = await _webPartService.GetUserWebPartsAsync(userId);
            var webPart = webParts.FirstOrDefault(w => w.Id == webPartId);
            if (webPart != null)
            {
                webPart.Width = width;
                webPart.Height = height;
                await _webPartService.UpdateWebPartAsync(webPart);
            }
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при обновлении размера {Id}", webPartId);
            return BadRequest("Ошибка при обновлении размера");
        }
    }

    [HttpPost]
    public async Task<IActionResult> UpdateWebPartTitle(int webPartId, string title)
    {
        try
        {
            var userId = GetCurrentUserId();
            var webParts = await _webPartService.GetUserWebPartsAsync(userId);
            var webPart = webParts.FirstOrDefault(w => w.Id == webPartId);
            if (webPart != null)
            {
                webPart.Title = title;
                await _webPartService.UpdateWebPartAsync(webPart);
            }
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при обновлении заголовка {Id}", webPartId);
            return BadRequest("Ошибка при обновлении заголовка");
        }
    }

    private int GetCurrentUserId()
    {
        var claim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        return claim != null ? int.Parse(claim.Value) : 1;
    }
}
