using Microsoft.AspNetCore.Mvc;
using THYLoggerAPI_POSTGRESQL.Model;
using THYLoggerAPI_POSTGRESQL.Services;

namespace THYLoggerAPI_POSTGRESQL.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NemController : ControllerBase
    {
        private readonly NemService _nemService;

        public NemController(NemService nemService)
        {
            _nemService = nemService;
        }

        // GET: api/Nem
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var list = await _nemService.GetAllAsync();
                return Ok(list);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Nem verileri alınırken bir hata oluştu: " + ex.Message);
            }
        }

        // POST: api/Nem  VEYA  POST: api/Nem/Add
        [HttpPost]
        [HttpPost("Add")]
        public async Task<IActionResult> Add([FromBody] Nem entity)
        {
            if (entity == null)
            {
                return BadRequest("Gönderilen veri boş olamaz.");
            }

            try
            {
                var result = await _nemService.AddAsync(entity);

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
                return StatusCode(500, "Ekleme sırasında bir hata oluştu: " + ex.Message);
            }
        }
    }
}