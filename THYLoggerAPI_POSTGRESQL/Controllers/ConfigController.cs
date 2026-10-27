using Microsoft.AspNetCore.Mvc;
using THYLoggerAPI_POSTGRESQL.DTOs;
using THYLoggerAPI_POSTGRESQL.Services;

namespace THYLoggerAPI_POSTGRESQL.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ConfigController : ControllerBase
    {
        private readonly ConfigService _configService;

        public ConfigController(ConfigService configService)
        {
            _configService = configService;
        }

        [HttpPost("Get")]
        public async Task<IActionResult> Get([FromBody] ConfigGetRequest request)
        {
            if (request == null) return BadRequest("Gönderilen veri boş olamaz.");

            try
            {
                var result = await _configService.GetAsync(request);
                if (!result.IsSuccess)
                    return result.IsNotFound ? NotFound(result.ErrorMessage) : BadRequest(result.ErrorMessage);

                return Ok(result.Response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Konfigürasyon alınırken bir hata oluştu: " + ex.Message);
            }
        }

        [HttpGet("GetVsControl")]
        public async Task<IActionResult> GetVsControl([FromQuery] ConfigVsControlRequest request)
        {
            try
            {
                var result = await _configService.VsControlAsync(request);
                if (!result.IsSuccess)
                    return result.IsNotFound ? NotFound(result.ErrorMessage) : BadRequest(result.ErrorMessage);

                return Ok(new { message = "Bildirim alındı" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Bildirim işlenirken bir hata oluştu: " + ex.Message);
            }
        }

        // ---- Dashboard uçları ----

        // Varsayılan config (ekran formunu doldurmak için)
        [HttpGet("Default")]
        public IActionResult Default() => Ok(new DeviceConfig());

        [HttpGet("Devices")]
        public async Task<IActionResult> Devices()
        {
            try
            {
                return Ok(await _configService.GetDevicesAsync());
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Cihaz listesi alınırken bir hata oluştu: " + ex.Message);
            }
        }

        [HttpPost("Publish")]
        public async Task<IActionResult> Publish([FromBody] ConfigPublishRequest request)
        {
            if (request == null) return BadRequest("Gönderilen veri boş olamaz.");

            try
            {
                var result = await _configService.PublishAsync(request);
                if (!result.IsSuccess) return BadRequest(result.ErrorMessage);

                return Ok(result.Response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Konfigürasyon yayınlanırken bir hata oluştu: " + ex.Message);
            }
        }

        // GET: api/Config/Events?dollyId=&eventType=&start=&end=&page=1&pageSize=50 (start/end Türkiye saati)
        [HttpGet("Events")]
        public async Task<IActionResult> Events(
            [FromQuery] int? dollyId, [FromQuery] string? eventType,
            [FromQuery] DateTime? start, [FromQuery] DateTime? end,
            [FromQuery] int page = 1, [FromQuery] int pageSize = 50)
        {
            try
            {
                return Ok(await _configService.GetEventsAsync(dollyId, eventType, start, end, page, pageSize));
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Olaylar alınırken bir hata oluştu: " + ex.Message);
            }
        }

        [HttpPost("DollyLog")]
        [RequestSizeLimit(ConfigService.MaxLogFileBytes + 1024 * 1024)]
        public async Task<IActionResult> DollyLog([FromForm] IFormFile? logFile, [FromForm] string? serialNumber)
        {
            try
            {
                var result = await _configService.SaveLogAsync(logFile, serialNumber);
                if (!result.IsSuccess)
                    return result.IsNotFound ? NotFound(result.ErrorMessage) : BadRequest(result.ErrorMessage);

                return Ok(new { message = "Log dosyası alındı", fileName = result.FileName });
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Log kaydedilirken bir hata oluştu: " + ex.Message);
            }
        }
    }
}
