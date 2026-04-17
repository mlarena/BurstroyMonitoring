using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BurstroyMonitoring.Data;
using BurstroyMonitoring.Data.Models;
using BurstroyMonitoring.Data.Models.ViewModels;
using BurstroyMonitoring.TCM.Services;
using System.Collections.Generic;

namespace BurstroyMonitoring.TCM.Controllers
{
    public class IwsDataController : BaseDataController<VwIwsDataFull>
    {
        public IwsDataController(
            ApplicationDbContext context,
            IExportService exportService,
            ILogger<IwsDataController> logger)
            : base(context, exportService, logger)
        {
        }

        protected override DbSet<VwIwsDataFull> DbSet => _context.VwIwsDataFull;

        protected override string FilterSessionKey => "IwsDataFilter";

        protected override Dictionary<string, string> GetRussianColumnNames()
        {
            return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["IwsDataId"] = "ID данных",
                ["ReceivedAt"] = "Время получения",
                ["SerialNumber"] = "Серийный номер",
                ["EnvironmentTemperature"] = "Температура воздуха",
                ["HumidityPercentage"] = "Относительная влажность воздуха",
                ["DewPoint"] = "Точка росы",
                ["PressureHpa"] = "Атмосферное давление (гПа)",
                ["PressureQnhHpa"] = "Давление, приведенное к уровню моря (QNH)",
                ["PressureMmhg"] = "Атмосферное давление (мм рт. ст.)",
                ["WindSpeed"] = "Скорость ветра",
                ["WindDirection"] = "Направление ветра",
                ["WindVsSound"] = "Скорость звука",
                ["PrecipitationType"] = "Тип осадков",
                ["PrecipitationIntensity"] = "Интенсивность осадков",
                ["PrecipitationQuantity"] = "Количество осадков",
                ["PrecipitationElapsed"] = "Накопленное время осадков",
                ["PrecipitationPeriod"] = "Период накопления осадков",
                ["PostName"] = "Имя поста",
                ["PostAddress"] = "Адрес поста",
                ["PostIsActive"] = "Пост активен"
            };
        }

        protected override List<string> GetAvailableFields<U>()
        {
            return new List<string>
            {
                "IwsDataId",
                "ReceivedAt",
                "SerialNumber",
                "EnvironmentTemperature",
                "HumidityPercentage",
                "DewPoint",
                "PressureHpa",
                "PressureQnhHpa",
                "PressureMmhg",
                "WindSpeed",
                "WindDirection",
                "WindVsSound",
                "PrecipitationType",
                "PrecipitationIntensity",
                "PrecipitationQuantity",
                "PrecipitationElapsed",
                "PrecipitationPeriod",
                "PostName",
                "PostAddress",
                "PostIsActive"
            };
        }

        protected override IQueryable<VwIwsDataFull> ApplySearch(
            IQueryable<VwIwsDataFull> query, 
            string search)
        {
            search = search.ToLower();
            return query.Where(e =>
                (e.SerialNumber != null && e.SerialNumber.ToLower().Contains(search)) ||
                (e.EndpointName != null && e.EndpointName.ToLower().Contains(search)) ||
                (e.PostName != null && e.PostName.ToLower().Contains(search)) ||
                (e.SensorTypeName != null && e.SensorTypeName.ToLower().Contains(search)));
        }

        protected override IQueryable<VwIwsDataFull> ApplyDefaultSort(
            IQueryable<VwIwsDataFull> query)
        {
            return query.OrderByDescending(e => e.IwsDataId);
        }
    }
}