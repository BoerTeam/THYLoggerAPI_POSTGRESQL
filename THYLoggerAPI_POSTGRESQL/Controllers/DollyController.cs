using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using THYLoggerAPI_POSTGRESQL.Context;
using THYLoggerAPI_POSTGRESQL.Model;

namespace THYLoggerAPI_POSTGRESQL.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DollyController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<DollyController> _logger;

        public DollyController(ApplicationDbContext context, ILogger<DollyController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/Dolly
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            _logger.LogInformation("Tüm Dolly verileri listeleniyor.");

            // Asenkron okuma sorgularında AsNoTracking() performansı ciddi oranda artırır
            var list = await _context.Dolly.AsNoTracking().OrderBy(i => i.Id).ToListAsync();
            return Ok(list);
        }

        // GET: api/Dolly/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            _logger.LogInformation("Dolly aranıyor. Aranacak ID: {DollyId}", id);

            var dolly = await _context.Dolly.FindAsync(id);
            if (dolly == null)
            {
                _logger.LogWarning("Dolly bulunamadı! Aranan ID: {DollyId}", id);
                return NotFound("Dolly bulunamadı.");
            }

            return Ok(dolly);
        }

        // POST: api/Dolly
        [HttpPost]
        public async Task<IActionResult> Add([FromBody] Dolly entity)
        {
            try
            {
                await _context.Dolly.AddAsync(entity);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Yeni Dolly verisi başarıyla eklendi. ID: {DollyId}", entity.Id);

                // REST standartlarına uygun olarak 201 Created ve oluşturulan nesne dönülür
                return CreatedAtAction(nameof(GetById), new { id = entity.Id }, entity);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Dolly eklenirken bir hata oluştu!");
                return BadRequest("Ekleme sırasında bir hata oluştu: " + ex.Message);
            }
        }

        // PUT: api/Dolly/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] Dolly entity)
        {
            if (id != entity.Id)
            {
                return BadRequest("URL'deki ID ile nesne ID'si uyuşmuyor.");
            }

            _logger.LogInformation("Dolly güncelleme isteği geldi. Güncellenecek ID: {DollyId}", id);

            var existingDolly = await _context.Dolly.FindAsync(id);
            if (existingDolly == null)
            {
                _logger.LogWarning("Güncellenmek istenen Dolly verisi bulunamadı. ID: {DollyId}", id);
                return NotFound("Güncellenecek kayıt bulunamadı.");
            }

            try
            {
                // Mevcut nesnenin değerlerini güncelliyoruz
                _context.Entry(existingDolly).CurrentValues.SetValues(entity);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Dolly verisi başarıyla güncellendi. ID: {DollyId}", id);
                return Ok(existingDolly);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Dolly güncellenirken bir hata oluştu! ID: {DollyId}", id);
                return BadRequest("Güncelleme sırasında bir hata oluştu: " + ex.Message);
            }
        }
    }
}