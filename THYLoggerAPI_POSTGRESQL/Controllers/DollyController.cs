using Microsoft.AspNetCore.Mvc;
using THYLoggerAPI_POSTGRESQL.Model;
using THYLoggerAPI_POSTGRESQL.Services;

namespace THYLoggerAPI_POSTGRESQL.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DollyController : ControllerBase
    {
        private readonly DollyService _dollyService;

        public DollyController(DollyService dollyService)
        {
            _dollyService = dollyService;
        }

        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var list = await _dollyService.GetAllAsync();
                return Ok(list);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Dolly verileri alınırken bir hata oluştu: " + ex.Message);
            }
        }

        [HttpGet("GetById/{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var dolly = await _dollyService.GetByIdAsync(id);
                if (dolly == null)
                {
                    return NotFound("Dolly bulunamadı.");
                }

                return Ok(dolly);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Dolly bilgisi alınırken bir hata oluştu: " + ex.Message);
            }
        }

        [HttpPost("Add")]
        public async Task<IActionResult> Add([FromBody] Dolly entity)
        {
            if (entity == null)
            {
                return BadRequest("Gönderilen Dolly verisi boş olamaz.");
            }

            try
            {
                var createdDolly = await _dollyService.AddAsync(entity);
                return CreatedAtAction(nameof(GetById), new { id = createdDolly.Id }, createdDolly);
            }
            catch (Exception ex)
            {
                return BadRequest("Ekleme sırasında bir hata oluştu: " + ex.Message);
            }
        }
        [HttpPut("Update/{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] Dolly entity)
        {
            if (entity == null || id != entity.Id)
            {
                return BadRequest("URL'deki ID ile nesne ID'si uyuşmuyor veya nesne boş.");
            }

            try
            {
                var updatedDolly = await _dollyService.UpdateAsync(id, entity);
                if (updatedDolly == null)
                {
                    return NotFound("Güncellenecek kayıt bulunamadı.");
                }

                return Ok(updatedDolly);
            }
            catch (Exception ex)
            {
                return BadRequest("Güncelleme sırasında bir hata oluştu: " + ex.Message);
            }
        }
    }
}