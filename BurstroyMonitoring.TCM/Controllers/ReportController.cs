using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using BurstroyMonitoring.Data;
using BurstroyMonitoring.Data.Models.ViewModels;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;
using Npgsql;

namespace BurstroyMonitoring.TCM.Controllers
{
    public class ReportController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReportController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var viewModel = new SensorSelectionViewModel();
            
            var monitoringPosts = await _context.MonitoringPosts
                .Where(mp => mp.IsActive)
                .OrderBy(mp => mp.Name)
                .ToListAsync();

            viewModel.MonitoringPosts = monitoringPosts
                .Select(mp => new SelectListItem
                {
                    Value = mp.Id.ToString(),
                    Text = mp.Name
                })
                .ToList();

            viewModel.MonitoringPosts.Insert(0, new SelectListItem
            {
                Value = "",
                Text = "Выберите пост мониторинга"
            });

            viewModel.Sensors = new List<SelectListItem>
            {
                new SelectListItem
                {
                    Value = "",
                    Text = "Сначала выберите пост мониторинга"
                }
            };

            return View(viewModel);
        }

        /// <summary>
        /// Получение данных для сводного отчета
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetReportData(int postId, string startDate, string endDate, int intervalMinutes = 10)
        {
            try
            {
                // Парсим даты. Используем инвариантную культуру для надежности
                if (!DateTime.TryParse(startDate, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out DateTime start))
                {
                    return BadRequest("Неверный формат даты начала");
                }
                
                if (!DateTime.TryParse(endDate, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out DateTime end))
                {
                    return BadRequest("Неверный формат даты окончания");
                }

                start = start.ToUniversalTime();
                end = end.AddDays(1).ToUniversalTime();
                
                var connectionString = _context.Database.GetConnectionString();
                
                // Структура для хранения данных: Время -> (Колонка -> Значение)
                var reportData = new SortedDictionary<DateTime, Dictionary<string, string>>();
                // Список всех уникальных колонок (Тип SN | Поле)
                var allColumns = new SortedSet<string>();

                using (var conn = new NpgsqlConnection(connectionString))
                {
                    await conn.OpenAsync();

                    // Описываем, какие поля из каких представлений нам нужны
                    var dataSources = new[]
                    {
                        new { Table = "vw_dov_data_full", Prefix = "DOV", Fields = new[] { "visible_range" } },
                        new { Table = "vw_dspd_data_full", Prefix = "DSPD", Fields = new[] { "grip_coefficient", "shake_level", "voltage_power", "case_temperature", "road_temperature", "water_height", "ice_height", "snow_height", "ice_percentage", "pgm_percentage", "road_status_code", "road_angle", "freeze_temperature", "distance_to_surface" } },
                        new { Table = "vw_dust_data_full", Prefix = "Dust", Fields = new[] { "pm10act", "pm25act", "pm1act", "pm10awg", "pm25awg", "pm1awg", "flowprobe", "temperatureprobe", "humidityprobe", "laserstatus", "supplyvoltage" } },
                        new { Table = "vw_iws_data_full", Prefix = "IWS", Fields = new[] { "environment_temperature", "humidity_percentage", "dew_point", "pressure_hpa", "pressure_qnh_hpa", "pressure_mmhg", "wind_speed", "wind_direction", "wind_vs_sound", "precipitation_type", "precipitation_intensity", "precipitation_quantity", "precipitation_elapsed", "precipitation_period", "co2_level" } },
                        new { Table = "vw_mueks_data_full", Prefix = "MUEKS", Fields = new[] { "temperature_box", "voltage_power_in_12b", "voltage_out_12b", "current_out_12b", "current_out_48b", "voltage_akb", "current_akb", "sensor_220b", "watt_hours_akb", "visible_range", "tds_h", "tds_tds", "tkosa_t1", "tkosa_t3" } }
                    };

                    foreach (var source in dataSources)
                    {
                        // Формируем SQL с агрегацией по интервалу времени
                        // Для MUEKS текстовые поля приводим к numeric с очисткой от пустых строк и 'NULL'
                        string fieldsSql = string.Join(", ", source.Fields.Select(f => 
                        {
                            if (source.Prefix == "MUEKS" && new[] { "tds_h", "tds_tds", "tkosa_t1", "tkosa_t3" }.Contains(f))
                                return $"ROUND(AVG(NULLIF(NULLIF(\"{f}\", ''), 'NULL')::numeric), 3) as \"{f}\"";
                            return $"ROUND(AVG(\"{f}\")::numeric, 3) as \"{f}\"";
                        }));
                        
                        // Используем арифметику эпохи для группировки по произвольному количеству минут
                        string sql = $@"
                            SELECT 
                                (TRUNC(EXTRACT(EPOCH FROM received_at) / ({intervalMinutes} * 60)) * ({intervalMinutes} * 60))::int as bucket,
                                serial_number,
                                {fieldsSql}
                            FROM public.{source.Table}
                            WHERE post_id = @postId AND received_at >= @start AND received_at < @end
                            GROUP BY bucket, serial_number
                            ORDER BY bucket DESC";

                        using (var cmd = new NpgsqlCommand(sql, conn))
                        {
                            cmd.Parameters.AddWithValue("postId", postId);
                            cmd.Parameters.AddWithValue("start", start);
                            cmd.Parameters.AddWithValue("end", end);

                            using (var reader = await cmd.ExecuteReaderAsync())
                            {
                                while (await reader.ReadAsync())
                                {
                                    // Преобразуем bucket (unix timestamp) обратно в DateTime
                                    var timestamp = reader.GetInt32(0);
                                    var time = DateTimeOffset.FromUnixTimeSeconds(timestamp).DateTime.ToLocalTime();
                                    var sn = reader.GetString(1);

                                    if (!reportData.ContainsKey(time))
                                        reportData[time] = new Dictionary<string, string>();

                                    foreach (var field in source.Fields)
                                    {
                                        // Получаем русское название поля или оставляем как есть
                                        string fieldDisplayName = GetFieldDisplayName(field);
                                        string fullColName = $"{source.Prefix} ({sn})|{fieldDisplayName}";
                                        allColumns.Add(fullColName);

                                        var valIdx = reader.GetOrdinal(field);
                                        reportData[time][fullColName] = reader.IsDBNull(valIdx) ? "-" : reader.GetValue(valIdx).ToString();
                                    }
                                }
                            }
                        }
                    }
                }

                // Формируем результат для JSON
                var result = new
                {
                    columns = allColumns.ToList(),
                    rows = reportData.Select(r => new
                    {
                        time = r.Key.ToString("dd.MM.yyyy HH:mm"),
                        values = r.Value
                    }).OrderByDescending(x => x.time).ToList()
                };

                return Json(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        private string GetFieldDisplayName(string field)
        {
            var fieldNames = new Dictionary<string, string>
            {
                { "visible_range", "Дальность видимости" },
                { "grip_coefficient", "Коэф. сцепления" },
                { "shake_level", "Уровень вибрации" },
                { "voltage_power", "Напряжение питания" },
                { "case_temperature", "Темп. внутри корпуса" },
                { "road_temperature", "Темп. покрытия" },
                { "water_height", "Высота воды" },
                { "ice_height", "Высота льда" },
                { "snow_height", "Высота снега" },
                { "ice_percentage", "% обледенения" },
                { "pgm_percentage", "% реагента" },
                { "road_status_code", "Код состояния" },
                { "road_angle", "Угол наклона" },
                { "freeze_temperature", "Темп. замерзания" },
                { "distance_to_surface", "Расстояние до пов." },
                { "pm10act", "PM10" },
                { "pm25act", "PM2.5" },
                { "pm1act", "PM1" },
                { "pm10awg", "PM10 сред." },
                { "pm25awg", "PM2.5 сред." },
                { "pm1awg", "PM1 сред." },
                { "flowprobe", "Расход воздуха" },
                { "temperatureprobe", "Темп. пробы" },
                { "humidityprobe", "Влажность пробы" },
                { "laserstatus", "Статус лазера" },
                { "supplyvoltage", "Напряжение" },
                { "environment_temperature", "Темп. воздуха" },
                { "humidity_percentage", "Влажность" },
                { "dew_point", "Точка росы" },
                { "pressure_hpa", "Давление (гПа)" },
                { "pressure_qnh_hpa", "Давление QNH" },
                { "pressure_mmhg", "Давление (мм рт. ст.)" },
                { "wind_speed", "Скорость ветра" },
                { "wind_direction", "Направление ветра" },
                { "wind_vs_sound", "Скорость звука" },
                { "precipitation_type", "Тип осадков" },
                { "precipitation_intensity", "Интенсивность осадков" },
                { "precipitation_quantity", "Кол-во осадков" },
                { "precipitation_elapsed", "Время осадков" },
                { "precipitation_period", "Период накопления" },
                { "co2_level", "Уровень CO2" },
                { "temperature_box", "Темп. в шкафу" },
                { "voltage_power_in_12b", "Вход 12В" },
                { "voltage_out_12b", "Выход 12В" },
                { "current_out_12b", "Ток вых. 12В" },
                { "current_out_48b", "Ток вых. 48В" },
                { "voltage_akb", "Напряжение АКБ" },
                { "current_akb", "Ток АКБ" },
                { "watt_hours_akb", "Емкость АКБ" },
                { "sensor_220b", "Питание 220В" },
                { "tds_h", "TDS H" },
                { "tds_tds", "TDS TDS" },
                { "tkosa_t1", "Tkosa T1" },
                { "tkosa_t3", "Tkosa T3" }
            };

            return fieldNames.ContainsKey(field) ? fieldNames[field] : field;
        }
    }
}
