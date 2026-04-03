using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BurstroyMonitoring.Data;
using BurstroyMonitoring.Data.Models;
using BurstroyMonitoring.Api.Models;

namespace BurstroyMonitoring.Api.Controllers
{
    [ApiController]
    [Route("api/v1/sensor-types")]
    public class SensorTypesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<SensorTypesController> _logger;

        public SensorTypesController(ApplicationDbContext context, ILogger<SensorTypesController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<IEnumerable<SensorType>>>> GetSensorTypes()
        {
            try
            {
                var types = await _context.SensorTypes.ToListAsync();
                return Ok(new ApiResponse<IEnumerable<SensorType>>
                {
                    Success = true,
                    Data = types
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting sensor types");
                return StatusCode(500, new ApiResponse<IEnumerable<SensorType>> { Success = false, Message = ex.Message });
            }
        }
    }
}
