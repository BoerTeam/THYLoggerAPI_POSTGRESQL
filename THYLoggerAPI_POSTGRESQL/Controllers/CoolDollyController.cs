using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using THYLoggerAPI_POSTGRESQL.DTOs;
using THYLoggerAPI_POSTGRESQL.Filters;
using THYLoggerAPI_POSTGRESQL.Services;

namespace THYLoggerAPI_POSTGRESQL.Controllers
{
    /// <summary>
    /// BPM -> Cool Dolly entegrasyon servisleri (Doküman v1.1).
    /// Tüm endpoint'ler JSON listesi ([ {...} ]) alır ve satır bazlı sonuç listesi döner.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [BpmBasicAuth]
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

        // POST /api/CoolDolly/CreateCoolDollyRequest
        [HttpPost("CreateCoolDollyRequest")]
        public async Task<IActionResult> CreateCoolDollyRequest([FromBody] List<CreateCoolDollyRequestDto>? dtos)
        {
            if (dtos == null || dtos.Count == 0)
            {
                return BadRequest(new[] { CoolDollyResponse.Error("CD-005", "İstek listesi boş olamaz") });
            }

            try
            {
                var results = await _service.CreateBatchAsync(dtos);
                return Ok(results);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "CreateCoolDollyRequest batch işlemi sırasında beklenmeyen sistem hatası!");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new[] { CoolDollyResponse.Error("CD-004", "Sistem hatası") });
            }
        }

        // POST /api/CoolDolly/UpdateCoolDollyRequest
        [HttpPost("UpdateCoolDollyRequest")]
        public async Task<IActionResult> UpdateCoolDollyRequest([FromBody] List<UpdateCoolDollyRequestDto>? dtos)
        {
            if (dtos == null || dtos.Count == 0)
            {
                return BadRequest(new[] { CoolDollyResponse.Error("CD-005", "İstek listesi boş olamaz") });
            }

            try
            {
                var results = await _service.UpdateBatchAsync(dtos);
                return Ok(results);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "UpdateCoolDollyRequest batch işlemi sırasında beklenmeyen sistem hatası!");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new[] { CoolDollyResponse.Error("CD-004", "Sistem hatası") });
            }
        }

        // POST /api/CoolDolly/TerminateCoolDollyRequest
        [HttpPost("TerminateCoolDollyRequest")]
        public async Task<IActionResult> TerminateCoolDollyRequest([FromBody] List<TerminateCoolDollyRequestDto>? dtos)
        {
            if (dtos == null || dtos.Count == 0)
            {
                return BadRequest(new[] { CoolDollyResponse.Error("CD-005", "İstek listesi boş olamaz") });
            }

            try
            {
                var results = await _service.TerminateBatchAsync(dtos);
                return Ok(results);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "TerminateCoolDollyRequest batch işlemi sırasında beklenmeyen sistem hatası!");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new[] { CoolDollyResponse.Error("CD-004", "Sistem hatası") });
            }
        }
    }
}