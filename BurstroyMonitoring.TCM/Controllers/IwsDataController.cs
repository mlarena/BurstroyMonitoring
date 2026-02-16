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
            IExportService exportService)
            : base(context, exportService)
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
                ["DataTimestamp"] = "Метка времени",
                ["EnvironmentTemperature"] = "Температура окружающей среды",
                ["HumidityPercentage"] = "Влажность воздуха",
                ["DewPoint"] = "Точка росы",
                ["PressureHpa"] = "Давление (гПа)",
                ["PressureQnhHpa"] = "Давление QNH (гПа)",
                ["PressureMmhg"] = "Давление (мм рт.ст.)",
                ["WindSpeed"] = "Скорость ветра",
                ["WindDirection"] = "Направление ветра",
                ["WindVsSound"] = "Ветер vs звук",
                ["PrecipitationType"] = "Тип осадков",
                ["PrecipitationIntensity"] = "Интенсивность осадков",
                ["PrecipitationQuantity"] = "Количество осадков",
                ["PrecipitationElapsed"] = "Продолжительность осадков",
                ["PrecipitationPeriod"] = "Период осадков",
                ["SupplyVoltage"] = "Напряжение питания",
                ["IwsLatitude"] = "Широта IWS",
                ["IwsLongitude"] = "Долгота IWS",
                ["Altitude"] = "Высота",
                ["KspValue"] = "Значение KSP",
                ["GpsSpeed"] = "Скорость GPS",
                ["AccelerationStdDev"] = "Стандартное отклонение ускорения",
                ["RollAngle"] = "Угол крена",
                ["PitchAngle"] = "Угол тангажа",
                ["StatusOk"] = "Статус ОК",
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