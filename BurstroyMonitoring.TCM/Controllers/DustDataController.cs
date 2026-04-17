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
            IExportService exportService,
            ILogger<DustDataController> logger)
            : base(context, exportService, logger)
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
                ["SerialNumber"] = "Серийный номер",
                ["PM10Act"] = "Концентрация PM10",
                ["PM25Act"] = "Концентрация PM2.5",
                ["PM1Act"] = "Концентрация PM1",
                ["PM10AWG"] = "Концентрация PM10 средняя",
                ["PM25AWG"] = "Концентрация PM2.5 средняя",
                ["PM1AWG"] = "Концентрация PM1 средняя",
                ["FlowProbe"] = "Расход воздуха через пробоотборник",
                ["TemperatureProbe"] = "Температура пробоотборника",
                ["HumidityProbe"] = "Влажность пробоотборника",
                ["PostName"] = "Имя поста",
                ["PostAddress"] = "Адрес поста",
                ["PostIsActive"] = "Пост активен"
            };
        }

        protected override List<string> GetAvailableFields<U>()
        {
            return new List<string>
            {
                "DustDataId",
                "ReceivedAt",
                "SerialNumber",
                "PM10Act",
                "PM25Act",
                "PM1Act",
                "PM10AWG",
                "PM25AWG",
                "PM1AWG",
                "FlowProbe",
                "TemperatureProbe",
                "HumidityProbe",
                "PostName",
                "PostAddress",
                "PostIsActive"
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