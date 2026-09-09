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

        // GET: api/Dolly
        [HttpGet]
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

        // GET: api/Dolly/5
        [HttpGet("{id}")]
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

        // POST: api/Dolly
        [HttpPost]
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

        // PUT: api/Dolly/5
        [HttpPut("{id}")]
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