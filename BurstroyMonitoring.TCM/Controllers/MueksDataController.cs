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
            IExportService exportService)
            : base(context, exportService)
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
                ["DataTimestamp"] = "Метка времени",
                ["TemperatureBox"] = "Температура в боксе",
                ["VoltagePowerIn12B"] = "Напряжение питания 12B",
                ["VoltageOut12B"] = "Напряжение на выходе 12B",
                ["CurrentOut12B"] = "Ток на выходе 12B",
                ["CurrentOut48B"] = "Ток на выходе 48B",
                ["VoltageAkb"] = "Напряжение АКБ",
                ["CurrentAkb"] = "Ток АКБ",
                ["Sensor220B"] = "Датчик 220B",
                ["WattHoursAkb"] = "Вт-часы АКБ",
                ["VisibleRange"] = "Видимая дальность",
                ["DoorStatus"] = "Статус двери",
                ["TdsH"] = "TDS H",
                ["TdsTds"] = "TDS значение",
                ["TkosaT1"] = "Ткоса T1",
                ["TkosaT3"] = "Ткоса T3",
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