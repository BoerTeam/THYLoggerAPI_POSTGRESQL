using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using THYLoggerAPI_POSTGRESQL.Context;
using THYLoggerAPI_POSTGRESQL.Model;

namespace THYLoggerAPI_POSTGRESQL.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DollyController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<DollyController> _logger; // 1. ILogger alanını ekledik

        // 2. Constructor üzerinden ILogger enjekte ettik
        public DollyController(ApplicationDbContext context, ILogger<DollyController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet("Get")]
        public IEnumerable<Dolly> Get()
        {
            _logger.LogInformation("Tüm Dolly verileri listeleniyor.");
            return _context.Dolly.OrderBy(i => i.Id).ToList();
        }

        [HttpPost("Add")]
        public IActionResult Add(Dolly entity)
        {
            try
            {
                _context.Dolly.Add(entity);
                _context.SaveChanges();

                // Parametreli log kullanımı (Structured Logging)
                _logger.LogInformation("Yeni Dolly verisi başarıyla eklendi. ID: {DollyId}", entity.Id);

                return Ok("Dolly Verisi Başarılı İle Eklendi");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Dolly eklenirken bir hata oluştu!");
                return BadRequest("Ekleme sırasında bir hata oluştu: " + ex.Message);
            }
        }

        [HttpGet("GetById/{id}")]
        public IActionResult GetById(int id)
        {
            _logger.LogInformation("Dolly aranıyor. Aranacak ID: {DollyId}", id);

            var dolly = _context.Dolly.FirstOrDefault(x => x.Id == id);
            if (dolly == null)
            {
                _logger.LogWarning("Dolly bulunamadı! Aranan ID: {DollyId}", id);
                return NotFound("Dolly bulunamadı.");
            }

            return Ok(dolly);
        }

        [HttpPut("Update")]
        public IActionResult Update(Dolly entity)
        {
            _logger.LogInformation("Dolly güncelleme isteği geldi. Güncellenecek ID: {DollyId}", entity.Id);

            var existingDolly = _context.Dolly.Any(x => x.Id == entity.Id);

            if (!existingDolly)
            {
                _logger.LogWarning("Güncellenmek istenen Dolly verisi bulunamadı. ID: {DollyId}", entity.Id);
                return NotFound("Güncellenecek kayıt bulunamadı.");
            }

            try
            {
                _context.Dolly.Update(entity);
                _context.SaveChanges();

                _logger.LogInformation("Dolly verisi başarıyla güncellendi. ID: {DollyId}", entity.Id);
                return Ok("Dolly verisi başarıyla güncellendi.");
            }
            catch (Exception ex)
            {
                // ex parametresini ilk sıraya koyarak exception detaylarını da DB'ye yazdırıyoruz
                _logger.LogError(ex, "Dolly güncellenirken bir hata oluştu! ID: {DollyId}", entity.Id);
                return BadRequest("Güncelleme sırasında bir hata oluştu: " + ex.Message);
            }
        }
    }
}