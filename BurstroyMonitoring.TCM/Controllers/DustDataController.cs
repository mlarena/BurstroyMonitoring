using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BurstroyMonitoring.Data;
using BurstroyMonitoring.Data.Models;
using BurstroyMonitoring.Data.Models.ViewModels;
using BurstroyMonitoring.TCM.Services;
using System.Collections.Generic;

namespace BurstroyMonitoring.TCM.Controllers
{
    public class DustDataController : BaseDataController<VwDustDataFull>
    {
        public DustDataController(
            ApplicationDbContext context,
            IExportService exportService)
            : base(context, exportService)
        {
        }

        protected override DbSet<VwDustDataFull> DbSet => _context.VwDustDataFull;

        protected override string FilterSessionKey => "DustDataFilter";

        protected override Dictionary<string, string> GetRussianColumnNames()
        {
            return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["DustDataId"] = "ID данных",
                ["ReceivedAt"] = "Время получения",
                ["DataTimestamp"] = "Метка времени",
                ["PM10Act"] = "PM10 (текущее)",
                ["PM25Act"] = "PM2.5 (текущее)",
                ["PM1Act"] = "PM1.0 (текущее)",
                ["PM10AWG"] = "PM10 (среднее)",
                ["PM25AWG"] = "PM2.5 (среднее)",
                ["PM1AWG"] = "PM1.0 (среднее)",
                ["FlowProbe"] = "Поток датчика",
                ["TemperatureProbe"] = "Температура датчика",
                ["HumidityProbe"] = "Влажность датчика",
                ["LaserStatus"] = "Статус лазера",
                ["SupplyVoltage"] = "Напряжение питания",
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

        protected override IQueryable<VwDustDataFull> ApplySearch(
            IQueryable<VwDustDataFull> query, 
            string search)
        {
            search = search.ToLower();
            return query.Where(e =>
                (e.SerialNumber != null && e.SerialNumber.ToLower().Contains(search)) ||
                (e.EndpointName != null && e.EndpointName.ToLower().Contains(search)) ||
                (e.PostName != null && e.PostName.ToLower().Contains(search)) ||
                (e.SensorTypeName != null && e.SensorTypeName.ToLower().Contains(search)));
        }

        protected override IQueryable<VwDustDataFull> ApplyDefaultSort(
            IQueryable<VwDustDataFull> query)
        {
            return query.OrderByDescending(e => e.DustDataId);
        }
    }
}