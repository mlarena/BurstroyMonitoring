using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BurstroyMonitoring.Data;
using BurstroyMonitoring.Data.Models;
using BurstroyMonitoring.Api.Models;

namespace BurstroyMonitoring.Api.Controllers
{
    [ApiController]
    [Route("api/v1/sensors")]
    public class SensorsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<SensorsController> _logger;

        public SensorsController(ApplicationDbContext context, ILogger<SensorsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<IEnumerable<Sensor>>>> GetSensors()
        {
            try
            {
                var sensors = await _context.Sensors.ToListAsync();
                return Ok(new ApiResponse<IEnumerable<Sensor>>
                {
                    Success = true,
                    Data = sensors
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting sensors");
                return StatusCode(500, new ApiResponse<IEnumerable<Sensor>> { Success = false, Message = ex.Message });
            }
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<ApiResponse<Sensor>>> GetSensor(int id)
        {
            try
            {
                var sensor = await _context.Sensors.FindAsync(id);
                if (sensor == null) 
                    return NotFound(new ApiResponse<Sensor> { Success = false, Message = $"Sensor with ID {id} not found" });

                return Ok(new ApiResponse<Sensor> { Success = true, Data = sensor });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting sensor {Id}", id);
                return StatusCode(500, new ApiResponse<Sensor> { Success = false, Message = ex.Message });
            }
        }    }
}
