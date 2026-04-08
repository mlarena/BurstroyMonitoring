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
        /// Получение стандартных метеоданных для конкретного датчика поста мониторинга
        /// </summary>
        /// <param name="monitoringPostId">ID поста мониторинга</param>
        /// <param name="sensorId">ID датчика</param>
        /// <returns>Метеоданные в стандартном формате</returns>
        [HttpGet("{monitoringPostId:int}/meteo-standart/{sensorId:int}")]
        public async Task<ActionResult<MeteoStandartResponse>> GetMeteoStandart(int monitoringPostId, int sensorId)
        {
            try
            {
                // Проверяем существование датчика и его принадлежность к посту
                var sensor = await _context.Sensors
                    .FirstOrDefaultAsync(s => s.Id == sensorId && s.MonitoringPostId == monitoringPostId);

                if (sensor == null)
                {
                    return NotFound(new { message = $"Sensor with ID {sensorId} not found for monitoring post {monitoringPostId}" });
                }

                // Получаем последнюю запись из IWSData для этого датчика
                var lastData = await _context.IWSData
                    .Where(d => d.SensorId == sensorId)
                    .OrderByDescending(d => d.DataTimestamp)
                    .FirstOrDefaultAsync();

                if (lastData == null)
                {
                    return Ok(new MeteoStandartResponse()); // Возвращаем пустой объект
                }

                // Маппинг данных в требуемый формат
                var response = new MeteoStandartResponse
                {
                    MeteoT_Air = (float?)lastData.EnvTemperature,
                    MeteoHumidity = (float?)lastData.Humidity,
                    MeteoAir_Pressure = (float?)lastData.PressureHPa,
                    MeteoWind_Velocity = (float?)lastData.WindSpeed,
                    MeteoWind_Gusts = -9999,
                    MeteoWind_Direction = (float?)lastData.WindDirection,
                    MeteoPrecip_Amount = (float?)lastData.PrecipitationQuantity,
                    MeteoPrecip_Intensity = (float?)lastData.PrecipitationIntensity,
                    MeteoView_Distance = -9999,
                    MeteoT_Road = -9999,
                    MeteoT_Underroad = -9999,
                    MeteoT_Base = -9999,
                    MeteoCondition_Road = -9999,
                    MeteoVolhumidity_Base = -9999,
                    MeteoLayer_Water = -9999,
                    MeteoSit_Intensity = lastData.PrecipitationType,
                    MeteoDew_Point = (float?)lastData.DewPoint,
                    MeteoLayer_Snow = -9999,
                    MeteoLayer_Ice = -9999,
                    MeteoPrecip_Code = lastData.PrecipitationType
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting meteo standart for sensor {SensorId}", sensorId);
                return StatusCode(500, new { message = "An error occurred while retrieving meteo data", error = ex.Message });
            }
        }
    }
}
