using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BurstroyMonitoring.Data;
using BurstroyMonitoring.Data.Models;
using BurstroyMonitoring.Data.Models.ViewModels;
using BurstroyMonitoring.TCM.Services;
using System.Collections.Generic;

namespace BurstroyMonitoring.TCM.Controllers
{
    public class DovDataController : BaseDataController<VwDovDataFull>
    {
        protected override string FilterSessionKey => "DovDataFilter";
        public DovDataController( ApplicationDbContext context, IExportService exportService)
            : base(context, exportService)
        {
        }

        protected override DbSet<VwDovDataFull> DbSet => _context.VwDovDataFull;
       
        protected override Dictionary<string, string> GetRussianColumnNames()
        {
            return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["DovDataId"]           = "ID данных",
                ["ReceivedAt"]          = "Время получения",
                ["DataTimestamp"]       = "Метка времени",
                ["VisibleRange"]        = "Видимая дальность",
                ["BrightFlag"]          = "Флаг яркости",
                ["SensorLongitude"]     = "Долгота датчика",
                ["SensorLatitude"]      = "Широта датчика",
                ["SerialNumber"]        = "Серийный номер",
                ["EndpointName"]        = "Конечная точка",
                ["SensorUrl"]           = "URL датчика",
                ["CheckIntervalSeconds"] = "Интервал проверки (сек)",
                ["LastActivityUtc"]     = "Последняя активность",
                ["SensorIsActive"]      = "Датчик активен",
                ["SensorTypeName"]      = "Тип датчика",
                ["SensorTypeDescription"] = "Описание типа",
                ["PostName"]            = "Имя поста",
                ["PostDescription"]     = "Описание поста",
                ["PostIsMobile"]        = "Мобильный пост",
                ["PostIsActive"]        = "Пост активен"
            };
        }

        protected override IQueryable<VwDovDataFull> ApplySearch(
            IQueryable<VwDovDataFull> query, 
            string search)
        {
            search = search.ToLower();
            return query.Where(e =>
                (e.SerialNumber != null && e.SerialNumber.ToLower().Contains(search)) ||
                (e.EndpointName != null && e.EndpointName.ToLower().Contains(search)) ||
                (e.PostName != null && e.PostName.ToLower().Contains(search)) ||
                (e.SensorTypeName != null && e.SensorTypeName.ToLower().Contains(search)));
        }

        protected override IQueryable<VwDovDataFull> ApplyDefaultSort(
            IQueryable<VwDovDataFull> query)
        {
            return query.OrderByDescending(e => e.DovDataId);
        }
    }
}