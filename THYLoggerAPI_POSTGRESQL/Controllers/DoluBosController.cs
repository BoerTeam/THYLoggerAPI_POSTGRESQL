using Microsoft.AspNetCore.Mvc;
using THYLoggerAPI_POSTGRESQL.Model;
using THYLoggerAPI_POSTGRESQL.Services;

namespace THYLoggerAPI_POSTGRESQL.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DoluBosController : ControllerBase
    {
        private readonly DoluBosService _doluBosService;

        public DoluBosController(DoluBosService doluBosService)
        {
            _doluBosService = doluBosService;
        }

        // GET: api/DoluBos
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var list = await _doluBosService.GetAllAsync();
                return Ok(list);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "BosDolu verileri alınırken sunucu hatası oluştu: " + ex.Message);
            }
        }

        // POST: api/DoluBos  VEYA  POST: api/DoluBos/Add
        [HttpPost]
        [HttpPost("Add")]
        public async Task<IActionResult> Add([FromBody] BosDolu entity)
        {
            if (entity == null)
            {
                return BadRequest("Gönderilen veri boş olamaz.");
            }

            try
            {
                var result = await _doluBosService.AddAsync(entity);

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
    }
}