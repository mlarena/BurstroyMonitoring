using BurstroyMonitoring.Data.Models;

namespace BurstroyMonitoring.TCM.Services;

public interface IWebPartService
{
    Task<List<WebPart>> GetUserWebPartsAsync(int userId);
    Task<WebPart> AddWebPartAsync(int userId, WebPartType type, string title);
    Task UpdateWebPartAsync(WebPart webPart);
    Task RemoveWebPartAsync(int webPartId);
}
