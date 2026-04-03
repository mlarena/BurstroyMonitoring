using BurstroyMonitoring.Data;
using BurstroyMonitoring.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace BurstroyMonitoring.TCM.Services;

public class WebPartService : IWebPartService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<WebPartService> _logger;

    public WebPartService(ApplicationDbContext context, ILogger<WebPartService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<WebPart>> GetUserWebPartsAsync(int userId)
    {
        var webParts = await _context.WebParts
            .Where(w => w.UserId == userId)
            .OrderBy(w => w.PositionY)
            .ThenBy(w => w.PositionX)
            .ToListAsync();

        if (!webParts.Any())
        {
            await InitializeDefaultWebPartsAsync(userId);
            webParts = await _context.WebParts
                .Where(w => w.UserId == userId)
                .OrderBy(w => w.PositionY)
                .ThenBy(w => w.PositionX)
                .ToListAsync();
        }

        return webParts;
    }

    private async Task InitializeDefaultWebPartsAsync(int userId)
    {
        var defaults = new List<WebPart>
        {
            new() { UserId = userId, Title = "Посты мониторинга", Type = WebPartType.MonitoringPosts,  PositionX = 0, PositionY = 0, Width = 6, Height = 5 },
            new() { UserId = userId, Title = "Карта",             Type = WebPartType.MonitoringMap,    PositionX = 6, PositionY = 0, Width = 6, Height = 5 },
            new() { UserId = userId, Title = "Данные датчиков",   Type = WebPartType.SensorData,       PositionX = 0, PositionY = 5, Width = 6, Height = 4 },
            new() { UserId = userId, Title = "Графики",           Type = WebPartType.GraphsAndCharts,  PositionX = 6, PositionY = 5, Width = 6, Height = 4 },
        };

        _context.WebParts.AddRange(defaults);
        await _context.SaveChangesAsync();
    }

    public async Task<WebPart> AddWebPartAsync(int userId, WebPartType type, string title)
    {
        var webPart = new WebPart
        {
            UserId = userId,
            Title = string.IsNullOrEmpty(title) ? GetDefaultTitle(type) : title,
            Type = type,
            PositionX = 0,
            PositionY = 100,
            Width = GetDefaultWidth(type),
            Height = GetDefaultHeight(type)
        };

        _context.WebParts.Add(webPart);
        await _context.SaveChangesAsync();
        return webPart;
    }

    public async Task UpdateWebPartAsync(WebPart webPart)
    {
        var existing = await _context.WebParts.FindAsync(webPart.Id);
        if (existing != null)
        {
            existing.Title = webPart.Title;
            existing.PositionX = webPart.PositionX;
            existing.PositionY = webPart.PositionY;
            existing.Width = webPart.Width;
            existing.Height = webPart.Height;
            existing.Settings = webPart.Settings;
            await _context.SaveChangesAsync();
        }
    }

    public async Task RemoveWebPartAsync(int webPartId)
    {
        var webPart = await _context.WebParts.FindAsync(webPartId);
        if (webPart != null)
        {
            _context.WebParts.Remove(webPart);
            await _context.SaveChangesAsync();
        }
    }

    private static string GetDefaultTitle(WebPartType type) => type switch
    {
        WebPartType.MonitoringPosts  => "Посты мониторинга",
        WebPartType.MonitoringMap    => "Карта",
        WebPartType.SensorData       => "Данные датчиков",
        WebPartType.GraphsAndCharts  => "Графики",
        WebPartType.Report           => "Отчеты",
        WebPartType.Cameras          => "Камеры",
        _                            => "Новая панель"
    };

    private static int GetDefaultWidth(WebPartType type) => type switch
    {
        WebPartType.MonitoringMap   => 6,
        WebPartType.GraphsAndCharts => 12,
        _                           => 6
    };

    private static int GetDefaultHeight(WebPartType type) => type switch
    {
        WebPartType.MonitoringMap   => 5,
        WebPartType.GraphsAndCharts => 5,
        _                           => 4
    };
}
