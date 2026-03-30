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
        /// Получение данных для сводного отчета.
        /// Собирает данные из 5 различных представлений, агрегирует их по времени и объединяет в одну таблицу.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetReportData(int postId, string startDate, string endDate, int intervalMinutes = 10)
        {
            try
            {
                // 1. Подготовка временных рамок.
                // Используем InvariantCulture для надежного парсинга ISO-даты (гггг-мм-дд) из flatpickr.
                if (!DateTime.TryParse(startDate, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out DateTime start))
                {
                    return BadRequest("Неверный формат даты начала");
                }
                
                if (!DateTime.TryParse(endDate, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out DateTime end))
                {
                    return BadRequest("Неверный формат даты окончания");
                }

                // Переводим в UTC для корректного сравнения с данными в БД (PostgreSQL хранит в UTC).
                // К конечной дате добавляем 1 день, чтобы захватить данные до конца выбранных суток.
                start = start.ToUniversalTime();
                end = end.AddDays(1).ToUniversalTime();
                
                var connectionString = _context.Database.GetConnectionString();
                
                // 2. Структуры для хранения результата.
                // reportData: Ключ - Время интервала, Значение - Словарь (Название колонки -> Значение).
                var reportData = new SortedDictionary<DateTime, Dictionary<string, string>>();
                
                // allColumns: Список всех уникальных колонок в порядке их обнаружения.
                // Используем List вместо SortedSet, чтобы колонки одного датчика шли подряд.
                var allColumns = new List<string>();

                using (var conn = new NpgsqlConnection(connectionString))
                {
                    await conn.OpenAsync();

                    // 3. Получаем список всех датчиков на этом посту, чтобы обработать каждый индивидуально.
                    var sensors = new List<(int Id, string Type, string Endpoint, string SN)>();
                    string sensorsSql = @"
                        SELECT s.""Id"", st.""SensorTypeName"", s.""EndPointsName"", s.""SerialNumber""
                        FROM ""Sensor"" s
                        JOIN ""SensorType"" st ON s.""SensorTypeId"" = st.""Id""
                        WHERE s.""MonitoringPostId"" = @postId AND s.""IsActive"" = true
                        ORDER BY st.""SensorTypeName"", s.""Id""";
                    
                    using (var cmd = new NpgsqlCommand(sensorsSql, conn))
                    {
                        cmd.Parameters.AddWithValue("postId", postId);
                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var sId = reader.GetInt32(0);
                                var sType = reader.GetString(1);
                                var sEnd = reader.GetString(2);
                                var sSN = reader.IsDBNull(3) ? "б/н" : reader.GetString(3);
                                sensors.Add((sId, sType, sEnd, sSN));
                                // Логирование в консоль сервера
                                Console.WriteLine($"[Report] Нашел датчик: ID={sId}, Тип={sType}, SN={sSN}");
                            }
                        }
                    }

                    // Словарь маппинга типов датчиков на таблицы и поля
                    // Ключи в ВЕРХНЕМ регистре для надежного сопоставления
                    var typeConfigs = new Dictionary<string, (string Table, string Prefix, string[] Fields)>
                    {
                        { "DOV", ("vw_dov_data_full", "DOV", new[] { "visible_range" }) },
                        { "DSPD", ("vw_dspd_data_full", "DSPD", new[] { "grip_coefficient", "shake_level", "voltage_power", "case_temperature", "road_temperature", "water_height", "ice_height", "snow_height", "ice_percentage", "pgm_percentage", "road_status_code", "road_angle", "freeze_temperature", "distance_to_surface" }) },
                        { "DUST", ("vw_dust_data_full", "DUST", new[] { "pm10act", "pm25act", "pm1act", "pm10awg", "pm25awg", "pm1awg", "flowprobe", "temperatureprobe", "humidityprobe", "laserstatus", "supplyvoltage" }) },
                        { "IWS", ("vw_iws_data_full", "IWS", new[] { "environment_temperature", "humidity_percentage", "dew_point", "pressure_hpa", "pressure_qnh_hpa", "pressure_mmhg", "wind_speed", "wind_direction", "wind_vs_sound", "precipitation_type", "precipitation_intensity", "precipitation_quantity", "precipitation_elapsed", "precipitation_period", "co2_level" }) },
                        { "MUEKS", ("vw_mueks_data_full", "MUEKS", new[] { "temperature_box", "voltage_power_in_12b", "voltage_out_12b", "current_out_12b", "current_out_48b", "voltage_akb", "current_akb", "sensor_220b", "watt_hours_akb", "tds_h", "tds_tds", "tkosa_t1", "tkosa_t3" }) }
                    };

                    // 4. Опрашиваем каждый датчик персонально
                    foreach (var sensor in sensors)
                    {
                        // Ищем конфигурацию (регистронезависимо)
                        var sensorTypeUpper = sensor.Type.ToUpper().Trim();
                        string matchedKey = typeConfigs.Keys.FirstOrDefault(k => sensorTypeUpper.Contains(k));
                        
                        if (matchedKey == null)
                        {
                            Console.WriteLine($"[Report] !!! НЕ НАЙДЕНА КОНФИГУРАЦИЯ для типа: '{sensor.Type}'");
                            continue;
                        }

                        var config = typeConfigs[matchedKey];
                        
                        // СРАЗУ добавляем колонки для этого датчика в общий список, 
                        // чтобы они отобразились в таблице, даже если данных в БД нет.
                        foreach (var field in config.Fields)
                        {
                            string fieldDisplayName = GetFieldDisplayName(field);
                            string fullColName = $"{config.Prefix} ({sensor.Endpoint} - {sensor.SN}) [ID:{sensor.Id}]|{fieldDisplayName}";
                            if (!allColumns.Contains(fullColName))
                                allColumns.Add(fullColName);
                        }

                        Console.WriteLine($"[Report] >>> Обработка датчика ID={sensor.Id}, Тип={sensor.Type}, Таблица={config.Table}");

                        string fieldsSql = string.Join(", ", config.Fields.Select(f => 
                        {
                            if (config.Prefix == "MUEKS" && new[] { "tds_h", "tds_tds", "tkosa_t1", "tkosa_t3" }.Contains(f))
                                return $"ROUND(AVG(NULLIF(NULLIF(\"{f}\", ''), 'NULL')::numeric), 3) as \"{f}\"";
                            return $"ROUND(AVG(\"{f}\")::numeric, 3) as \"{f}\"";
                        }));


                        string sql = $@"
                            SELECT 
                                (TRUNC(EXTRACT(EPOCH FROM received_at) / ({intervalMinutes} * 60)) * ({intervalMinutes} * 60))::int as bucket,
                                {fieldsSql}
                            FROM public.{config.Table}
                            WHERE sensor_id = @sensorId AND received_at >= @start AND received_at < @end
                            GROUP BY bucket
                            ORDER BY bucket DESC";

                        using (var cmd = new NpgsqlCommand(sql, conn))
                        {
                            cmd.Parameters.AddWithValue("sensorId", sensor.Id);
                            cmd.Parameters.AddWithValue("start", start);
                            cmd.Parameters.AddWithValue("end", end);

                            using (var reader = await cmd.ExecuteReaderAsync())
                            {
                                while (await reader.ReadAsync())
                                {
                                    var timestamp = reader.GetInt32(0);
                                    var time = DateTimeOffset.FromUnixTimeSeconds(timestamp).DateTime.ToLocalTime();
                                    
                                    if (!reportData.ContainsKey(time))
                                        reportData[time] = new Dictionary<string, string>();

                                    foreach (var field in config.Fields)
                                    {
                                        string fieldDisplayName = GetFieldDisplayName(field);
                                        string fullColName = $"{config.Prefix} ({sensor.Endpoint} - {sensor.SN}) [ID:{sensor.Id}]|{fieldDisplayName}";
                                        
                                        if (!allColumns.Contains(fullColName))
                                            allColumns.Add(fullColName);

                                        var valIdx = reader.GetOrdinal(field);
                                        reportData[time][fullColName] = reader.IsDBNull(valIdx) ? "-" : reader.GetValue(valIdx).ToString();
                                    }
                                }
                            }
                        }
                    }
                }

                // 6. Формирование финального JSON.
                // Преобразуем SortedDictionary в список объектов, понятных для JavaScript.
                // Каждая строка содержит время и словарь значений для всех колонок всех датчиков.
                var resultRows = reportData.Select(r => new
                {
                    time = r.Key.ToString("dd.MM.yyyy HH:mm"),
                    values = r.Value
                }).OrderByDescending(x => x.time).ToList();

                return Json(new
                {
                    columns = allColumns.ToList(),
                    rows = resultRows
                });
            }
            catch (Exception ex)
            {
                // В случае ошибки возвращаем 500 статус и текст ошибки для отладки на фронтенде.
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
