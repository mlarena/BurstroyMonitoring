using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BurstroyMonitoring.Data;
using BurstroyMonitoring.Data.Models;
using BurstroyMonitoring.Data.Models.ViewModels;
using BurstroyMonitoring.TCM.Services;
using System.Collections.Generic;

namespace BurstroyMonitoring.TCM.Controllers
{
    public class MueksDataController : BaseDataController<VwMueksDataFull>
    {
        public MueksDataController(
            ApplicationDbContext context,
            IExportService exportService,
            ILogger<MueksDataController> logger)
            : base(context, exportService, logger)
        {
        }

        protected override DbSet<VwMueksDataFull> DbSet => _context.VwMueksDataFull;

        protected override string FilterSessionKey => "MueksDataFilter";

        protected override Dictionary<string, string> GetRussianColumnNames()
        {
            return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["MueksDataId"] = "ID данных",
                ["ReceivedAt"] = "Время получения",
                ["SerialNumber"] = "Серийный номер",
                ["TemperatureBox"] = "Температура внутри шкафа",
                ["VoltagePowerIn12B"] = "Входное напряжение 12В",
                ["VoltageOut12B"] = "Выходное напряжение 12В",
                ["VoltageAkb"] = "Напряжение аккумуляторной батареи",
                ["CurrentOut12B"] = "Выходной ток 12В",
                ["CurrentOut48B"] = "Выходной ток 48В",
                ["CurrentAkb"] = "Ток аккумуляторной батареи",
                ["WattHoursAkb"] = "Емкость аккумуляторной батареи",
                ["VisibleRange"] = "Дальность видимости",
                ["Sensor220B"] = "Наличие питания 220В",
                ["DoorStatus"] = "Состояние двери",
                ["PostName"] = "Имя поста",
                ["PostAddress"] = "Адрес поста",
                ["PostIsActive"] = "Пост активен"
            };
        }

        protected override List<string> GetAvailableFields<U>()
        {
            return new List<string>
            {
                "MueksDataId",
                "ReceivedAt",
                "SerialNumber",
                "TemperatureBox",
                "VoltagePowerIn12B",
                "VoltageOut12B",
                "VoltageAkb",
                "CurrentOut12B",
                "CurrentOut48B",
                "CurrentAkb",
                "WattHoursAkb",
                "VisibleRange",
                "Sensor220B",
                "DoorStatus",
                "PostName",
                "PostAddress",
                "PostIsActive"
            };
        }

        protected override IQueryable<VwMueksDataFull> ApplySearch(
            IQueryable<VwMueksDataFull> query, 
            string search)
        {
            search = search.ToLower();
            return query.Where(e =>
                (e.SerialNumber != null && e.SerialNumber.ToLower().Contains(search)) ||
                (e.EndpointName != null && e.EndpointName.ToLower().Contains(search)) ||
                (e.PostName != null && e.PostName.ToLower().Contains(search)) ||
                (e.SensorTypeName != null && e.SensorTypeName.ToLower().Contains(search)));
        }

        protected override IQueryable<VwMueksDataFull> ApplyDefaultSort(
            IQueryable<VwMueksDataFull> query)
        {
            return query.OrderByDescending(e => e.MueksDataId);
        }
    }
}