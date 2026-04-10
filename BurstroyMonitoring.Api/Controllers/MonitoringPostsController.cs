using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BurstroyMonitoring.Data;
using BurstroyMonitoring.Data.Models;
using BurstroyMonitoring.Api.Models;

namespace BurstroyMonitoring.Api.Controllers
{
    [ApiController]
    [Route("api/v1/monitoring-posts")]
    public class MonitoringPostsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<MonitoringPostsController> _logger;

        public MonitoringPostsController(ApplicationDbContext context, ILogger<MonitoringPostsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetMonitoringPosts()
        {
            try
            {
                var posts = await _context.MonitoringPosts.ToListAsync();
                return Ok(posts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting monitoring posts");
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetMonitoringPost(int id)
        {
            try
            {
                var post = await _context.MonitoringPosts.FindAsync(id);
                if (post == null)
                    return NotFound(new { message = $"Monitoring post with ID {id} not found" });

                return Ok(post);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting monitoring post {Id}", id);
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpGet("{id:int}/sensors")]
        public async Task<IActionResult> GetSensorsByPost(int id)
        {
            try
            {
                var postExists = await _context.MonitoringPosts.AnyAsync(p => p.Id == id);
                if (!postExists)
                    return NotFound(new { message = $"Monitoring post with ID {id} not found" });

                var sensors = await _context.Sensors
                    .Where(s => s.MonitoringPostId == id)
                    .ToListAsync();

                return Ok(sensors);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting sensors for monitoring post {Id}", id);
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpGet("{monitoringPostId:int}/sensors/{sensorId:int}/results/latest/response-body")]
        public async Task<IActionResult> GetLatestResponseBody(int monitoringPostId, int sensorId)
        {
            try
            {
                var postExists = await _context.MonitoringPosts.AnyAsync(p => p.Id == monitoringPostId);
                if (!postExists)
                    return NotFound(new { message = $"Monitoring post with ID {monitoringPostId} not found" });

                var sensorExists = await _context.Sensors.AnyAsync(s => s.Id == sensorId && s.MonitoringPostId == monitoringPostId);
                if (!sensorExists)
                    return NotFound(new { message = $"Sensor with ID {sensorId} not found in monitoring post {monitoringPostId}" });

                var latestResult = await _context.SensorResults
                    .Where(r => r.SensorId == sensorId)
                    .OrderByDescending(r => r.CheckedAt)
                    .Select(r => r.ResponseBody)
                    .FirstOrDefaultAsync();

                if (latestResult == null)
                    return NotFound(new { message = "Latest result not found for this sensor" });

                return Ok(latestResult);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting latest response body for sensor {SensorId}", sensorId);
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpGet("{monitoringPostId:int}/sensors/{sensorId:int}/error")]
        public async Task<IActionResult> GetSensorErrors(int monitoringPostId, int sensorId)
        {
            try
            {
                var postExists = await _context.MonitoringPosts.AnyAsync(p => p.Id == monitoringPostId);
                if (!postExists)
                    return NotFound(new { message = $"Monitoring post with ID {monitoringPostId} not found" });

                var sensorExists = await _context.Sensors.AnyAsync(s => s.Id == sensorId && s.MonitoringPostId == monitoringPostId);
                if (!sensorExists)
                    return NotFound(new { message = $"Sensor with ID {sensorId} not found in monitoring post {monitoringPostId}" });

                var errors = await _context.SensorError
                    .Where(e => e.SensorId == sensorId)
                    .OrderByDescending(e => e.CreatedAt)
                    .ToListAsync();

                return Ok(errors);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting errors for sensor {SensorId}", sensorId);
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpGet("{monitoringPostId:int}/sensors/{sensorId:int}/id")]
        public async Task<IActionResult> GetSensorInPost(int monitoringPostId, int sensorId)
        {
            try
            {
                var postExists = await _context.MonitoringPosts.AnyAsync(p => p.Id == monitoringPostId);
                if (!postExists)
                    return NotFound(new { message = $"Monitoring post with ID {monitoringPostId} not found" });

                var sensor = await _context.Sensors
                    .FirstOrDefaultAsync(s => s.Id == sensorId && s.MonitoringPostId == monitoringPostId);

                if (sensor == null)
                    return NotFound(new { message = $"Sensor with ID {sensorId} not found in monitoring post {monitoringPostId}" });

                return Ok(sensor);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting sensor {SensorId} in post {PostId}", sensorId, monitoringPostId);
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>
        /// Получение стандартных метеоданных для поста мониторинга
        /// </summary>
        /// <param name="monitoringPostId">ID поста мониторинга</param>
        /// <returns>Метеоданные в стандартном формате</returns>
        [HttpGet("{monitoringPostId:int}/meteo-standart")]
        public async Task<ActionResult<MeteoStandartResponse>> GetMeteoStandart(int monitoringPostId)
        {
            try
            {
                // Получаем последнюю запись из представления vw_meteo_standart_last для этого поста
                var lastData = await _context.VwMeteoStandartLast
                    .Where(d => d.MonitoringPostId == monitoringPostId)
                    .FirstOrDefaultAsync();

                if (lastData == null)
                {
                    return Ok(new MeteoStandartResponse()); // Возвращаем пустой объект
                }
                // там где стоит ?? -9999 мы ранее не знали откуда брать значения. не убирать и быть внимательным к этим полям
                // там где стоит -9999 bмы и сейчас не знам откуда брать данные
                // Маппинг данных в требуемый формат
                var response = new MeteoStandartResponse
                {
                    meteo_t_air  = (float?)lastData.EnvTemperature,                          // IWS Температура воздуха, °C 
                    meteo_humidity = (float?)lastData.Humidity,                              // IWS Относительная влажность воздуха, %
                    meteo_air_pressure = (float?)lastData.PressureHPa,                       // IWS Атмосферное давление, гПа
                    meteo_wind_velocity = (float?)lastData.WindSpeed,                        // IWS Скорость ветра, м/с 
                    meteo_wind_gusts = (float?)lastData.WindSpeed,                           // IWS ??? Порывы ветра, м/с 
                    meteo_wind_direction = (float?)lastData.WindDirection,                   // IWS Направление ветра, град 
                    meteo_precip_amount = (float?)lastData.PrecipitationQuantity,            // IWS Количество осадков, мм
                    meteo_precip_intensity = (float?)lastData.PrecipitationIntensity,        // IWS Интенсивность осадков, мм/ч 
                    meteo_view_distance = (int?)lastData.VisibleRange ?? -9999,              // DOV "visibleRange": "Дальность видимости" Метеорологическая дальность видимости, м 
                    meteo_t_road = (float?)lastData.TemperatureRoad ?? -9999,                // DSPD "roadTemperature": "Температура дорожного покрытия" Температура поверхности дорожного покрытия, °C 
                    meteo_t_underroad  =  -9999,                                             // DSPD TemperatureCase Температура дорожной одежды , °C 
                    meteo_t_base = -9999,                                                    // Температура грунта земляного полотна, °C 
                    meteo_condition_road = lastData.RoadStatus ?? -9999,                     // DSPD RoadStatus Код состояния поверхности дороги:
                                                                                             //  1 — сухо;
                                                                                             //  2 — мокро (вода);
                                                                                             //  3 — лед;
                                                                                             //  4 — реагент;
                                                                                             //  5 — реагент со льдом
                    meteo_volhumidity_base  = -9999,                                         //  Объемная влажность дорожной одежды, % 
                    meteo_layer_water = (float?)lastData.HeightH2O ?? -9999,                 //  "waterHeight": "Высота слоя воды"  Высота слоя воды на поверхности, мм
                    meteo_sit_intensity = lastData.PrecipitationIntensity > 0 ? 1 : 0,       //  IWS Наличие осадков:
                                                                                             //  0 — нет;
                                                                                             //  1 — да
                    meteo_dew_point = (float?)lastData.DewPoint,                             // IWS Температура точки росы, °C
                    meteo_layer_snow = (float?)lastData.HeightSnow ?? -9999,                 // DSPD "snowHeight": "Высота слоя снега", Высота слоя снега на поверхности (опционально), мм 
                    meteo_layer_ice = (float?)lastData.HeightIce ?? -9999,                   // DSPD "iceHeight": "Высота слоя льда", Высота слоя льда на поверхности (опционально), мм 
                    meteo_precip_code  = lastData.PrecipitationType                          // IWS Код осадков:
                                                                                             //  1 — дождь;
                                                                                             //  2 — дождь со снегом;
                                                                                             //  3 — снег
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting meteo standart for post {PostId}", monitoringPostId);
                return StatusCode(500, new { message = "An error occurred while retrieving meteo data", error = ex.Message });
            }
        }

    }
}
