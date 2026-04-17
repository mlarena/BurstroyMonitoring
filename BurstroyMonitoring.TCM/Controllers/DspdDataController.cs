using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BurstroyMonitoring.Data;
using BurstroyMonitoring.Data.Models;
using BurstroyMonitoring.Data.Models.ViewModels;
using BurstroyMonitoring.TCM.Services;
using System.Collections.Generic;

namespace BurstroyMonitoring.TCM.Controllers
{
    public class DspdDataController : BaseDataController<VwDspdDataFull>
    {
        public DspdDataController(
            ApplicationDbContext context,
            IExportService exportService,
            ILogger<DspdDataController> logger)
            : base(context, exportService, logger)
        {
        }

        protected override DbSet<VwDspdDataFull> DbSet => _context.VwDspdDataFull;

        // ← Вот это исправление: переопределяем абстрактное свойство
        protected override string FilterSessionKey => "DspdDataFilter";

        protected override Dictionary<string, string> GetRussianColumnNames()
        {
            return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["DspdDataId"] = "ID данных",
                ["ReceivedAt"] = "Время получения",
                ["SerialNumber"] = "Серийный номер",
                ["GripCoefficient"] = "Коэффициент сцепления",
                ["ShakeLevel"] = "Уровень вибрации",
                ["VoltagePower"] = "Напряжение питания",
                ["CaseTemperature"] = "Температура внутри корпуса",
                ["RoadTemperature"] = "Температура дорожного покрытия",
                ["WaterHeight"] = "Высота слоя воды",
                ["IceHeight"] = "Высота слоя льда",
                ["SnowHeight"] = "Высота слоя снега",
                ["IcePercentage"] = "Процент обледенения",
                ["PgmPercentage"] = "Процент реагента",
                ["RoadStatusCode"] = "Код состояния дороги",
                ["FreezeTemperature"] = "Температура замерзания",
                ["DistanceToSurface"] = "Расстояние до поверхности",
                ["PostName"] = "Имя поста",
                ["PostAddress"] = "Адрес поста",
                ["PostIsActive"] = "Пост активен"
            };
        }

        protected override List<string> GetAvailableFields<U>()
        {
            return new List<string>
            {
                "DspdDataId",
                "ReceivedAt",
                "SerialNumber",
                "GripCoefficient",
                "ShakeLevel",
                "VoltagePower",
                "CaseTemperature",
                "RoadTemperature",
                "WaterHeight",
                "IceHeight",
                "SnowHeight",
                "IcePercentage",
                "PgmPercentage",
                "RoadStatusCode",
                "FreezeTemperature",
                "DistanceToSurface",
                "PostName",
                "PostAddress",
                "PostIsActive"
            };
        }

        protected override IQueryable<VwDspdDataFull> ApplySearch(
            IQueryable<VwDspdDataFull> query, 
            string search)
        {
            search = search.ToLower();
            return query.Where(e =>
                (e.SerialNumber != null && e.SerialNumber.ToLower().Contains(search)) ||
                (e.EndpointName != null && e.EndpointName.ToLower().Contains(search)) ||
                (e.PostName != null && e.PostName.ToLower().Contains(search)) ||
                (e.SensorTypeName != null && e.SensorTypeName.ToLower().Contains(search)));
        }

        protected override IQueryable<VwDspdDataFull> ApplyDefaultSort(
            IQueryable<VwDspdDataFull> query)
        {
            return query.OrderByDescending(e => e.DspdDataId);
        }
    }
}