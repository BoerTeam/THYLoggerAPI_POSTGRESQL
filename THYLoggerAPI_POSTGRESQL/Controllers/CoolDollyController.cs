using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using THYLoggerAPI_POSTGRESQL.DTOs;
using THYLoggerAPI_POSTGRESQL.Filters;
using THYLoggerAPI_POSTGRESQL.Services;

namespace THYLoggerAPI_POSTGRESQL.Controllers
{
    /// <summary>
    /// BPM -> Cool Dolly entegrasyon servisleri.
    /// Cevap formatı servis mapping dokümanındaki (status/message/referenceNumber/timestamp) yapıdadır.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [ApiKeyAuth]
    [EnableRateLimiting("bpm-limit")]
    public class CoolDollyController : ControllerBase
    {
        private readonly CoolDollyRequestService _service;
        private readonly ILogger<CoolDollyController> _logger;

        public CoolDollyController(CoolDollyRequestService service, ILogger<CoolDollyController> logger)
        {
            _service = service;
            _logger = logger;
        }

        // createCoolDollyRequest  ->  POST /api/CoolDolly/CreateRequest
        [HttpPost("CreateRequest")]
        public async Task<IActionResult> CreateRequest([FromBody] CreateCoolDollyRequestDto dto)
        {
            try
            {
                var result = await _service.CreateAsync(dto);
                return Ok(result);
            }
            catch (CoolDollyException ex)
            {
                return StatusCode(ex.StatusCode, CoolDollyResponse.Error(ex.Code, ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "createCoolDollyRequest sırasında beklenmeyen hata!");
                var sys = CoolDollyException.SystemError();
                return StatusCode(sys.StatusCode, CoolDollyResponse.Error(sys.Code, sys.Message));
            }
        }

        // terminateCoolDollyRequest  ->  POST /api/CoolDolly/TerminateRequest
        [HttpPost("TerminateRequest")]
        public async Task<IActionResult> TerminateRequest([FromBody] TerminateCoolDollyRequestDto dto)
        {
            try
            {
                var result = await _service.TerminateAsync(dto);
                return Ok(result);
            }
            catch (CoolDollyException ex)
            {
                return StatusCode(ex.StatusCode, CoolDollyResponse.Error(ex.Code, ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "terminateCoolDollyRequest sırasında beklenmeyen hata!");
                var sys = CoolDollyException.SystemError();
                return StatusCode(sys.StatusCode, CoolDollyResponse.Error(sys.Code, sys.Message));
            }
        }
    }
}
