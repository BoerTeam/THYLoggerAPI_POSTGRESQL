using Microsoft.AspNetCore.Mvc;
using THYLoggerAPI_POSTGRESQL.Model;
using THYLoggerAPI_POSTGRESQL.Services;

namespace THYLoggerAPI_POSTGRESQL.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SicaklikController : ControllerBase
    {
        private readonly SicaklikService _sicaklikService;

        public SicaklikController(SicaklikService sicaklikService)
        {
            _sicaklikService = sicaklikService;
        }

        // GET: api/Sicaklik
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var list = await _sicaklikService.GetAllAsync();
                return Ok(list);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Sıcaklık verileri alınırken bir hata oluştu: " + ex.Message);
            }
        }

        // POST: api/Sicaklik  VEYA  POST: api/Sicaklik/Add
        [HttpPost]
        [HttpPost("Add")]
        public async Task<IActionResult> Add([FromBody] Sicaklik entity)
        {
            try
            {
                var result = await _sicaklikService.AddAsync(entity);

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