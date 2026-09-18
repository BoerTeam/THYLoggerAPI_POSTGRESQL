using Microsoft.AspNetCore.Mvc;
using THYLoggerAPI_POSTGRESQL.Model;
using THYLoggerAPI_POSTGRESQL.Services;

namespace THYLoggerAPI_POSTGRESQL.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GpsController : ControllerBase
    {
        private readonly GpsService _gpsService;

        public GpsController(GpsService gpsService)
        {
            _gpsService = gpsService;
        }

        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var list = await _gpsService.GetAllAsync();
                return Ok(list);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "GPS verileri alınırken bir hata oluştu: " + ex.Message);
            }
        }

        
        [HttpPost("Add")]
        public async Task<IActionResult> Add([FromBody] Gpsdatum entity)
        {
            try
            {
                var result = await _gpsService.AddAsync(entity);

                if (!result.IsSuccess)
                {
                    if (result.IsNotFound)
                    {
                        return NotFound(result.ErrorMessage);
                    }

                    return BadRequest(result.ErrorMessage);
                }

                return Ok(result.ResponseData);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Sunucu hatası: " + ex.Message);
            }
        }

        [HttpGet("History/{id}")]
        public async Task<IActionResult> GetHistoryData(int id, [FromQuery] DateTime? start, [FromQuery] DateTime? end)
        {
            try
            {
                var history = await _gpsService.GetHistoryDataAsync(id, start, end);
                return Ok(history);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Geçmiş veriler alınırken bir hata oluştu: " + ex.Message);
            }
        }
    }
}