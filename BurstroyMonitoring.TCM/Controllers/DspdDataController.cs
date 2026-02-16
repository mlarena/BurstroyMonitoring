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
            IExportService exportService)
            : base(context, exportService)
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
                ["DataTimestamp"] = "Метка времени",
                ["GripCoefficient"] = "Коэффициент сцепления",
                ["ShakeLevel"] = "Уровень вибрации",
                ["VoltagePower"] = "Напряжение питания",
                ["CaseTemperature"] = "Температура корпуса",
                ["RoadTemperature"] = "Температура дороги",
                ["WaterHeight"] = "Высота воды",
                ["IceHeight"] = "Высота льда",
                ["SnowHeight"] = "Высота снега",
                ["IcePercentage"] = "Процент льда",
                ["PgmPercentage"] = "Процент ПГМ",
                ["RoadStatusCode"] = "Статус дороги",
                ["RoadAngle"] = "Угол дороги",
                ["FreezeTemperature"] = "Температура замерзания",
                ["CalibrationNeeded"] = "Требуется калибровка",
                ["GpsLatitude"] = "Широта GPS",
                ["GpsLongitude"] = "Долгота GPS",
                ["GpsValid"] = "GPS валиден",
                ["DistanceToSurface"] = "Расстояние до поверхности",
                ["SensorLongitude"] = "Долгота датчика",
                ["SensorLatitude"] = "Широта датчика",
                ["SerialNumber"] = "Серийный номер",
                ["EndpointName"] = "Конечная точка",
                ["SensorUrl"] = "URL датчика",
                ["CheckIntervalSeconds"] = "Интервал проверки (сек)",
                ["LastActivityUtc"] = "Последняя активность",
                ["SensorIsActive"] = "Датчик активен",
                ["SensorTypeName"] = "Тип датчика",
                ["SensorTypeDescription"] = "Описание типа",
                ["PostName"] = "Имя поста",
                ["PostDescription"] = "Описание поста",
                ["PostIsMobile"] = "Мобильный пост",
                ["PostIsActive"] = "Пост активен"
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