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

        // GET: api/Gps
        [HttpGet]
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

        // POST: api/Gps  VEYA  POST: api/Gps/Add
        // Cihaz "api/Gps/Add" adresine istek attığı için [HttpPost("Add")] eklendi
        [HttpPost]
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

        // GET: api/Gps/History/5?start=2026-09-01T00:00:00&end=2026-09-09T00:00:00
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