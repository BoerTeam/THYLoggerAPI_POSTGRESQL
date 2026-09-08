using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using THYLoggerAPI_POSTGRESQL.Context;
using THYLoggerAPI_POSTGRESQL.Model;

namespace THYLoggerAPI_POSTGRESQL.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DoluBosController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<DoluBosController> _logger;

        public DoluBosController(ApplicationDbContext context, ILogger<DoluBosController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            _logger.LogInformation("Tüm BosDolu verileri listeleniyor.");

            var list = await _context.BosDolu
                .AsNoTracking()
                .OrderBy(i => i.Id)
                .ToListAsync();

            return Ok(list);
        }

        [HttpPost]
        public async Task<IActionResult> Add([FromBody] BosDolu entity)
        {
            // 1. Seri numarası gönderilmiş mi kontrol et
            if (string.IsNullOrWhiteSpace(entity.SerialNumber))
            {
                _logger.LogWarning("SerialNumber gönderilmedi.");
                return BadRequest("SerialNumber gönderilmesi zorunludur.");
            }

            // 2. Veritabanında bu seri numarasına sahip Dolly'yi asenkron bul
            var dolly = await _context.Dolly
                .FirstOrDefaultAsync(x => x.SerialNumber == entity.SerialNumber);

            if (dolly == null)
            {
                _logger.LogWarning("'{SerialNumber}' seri numaralı cihaz sistemde kayıtlı değil.", entity.SerialNumber);
                return NotFound($"'{entity.SerialNumber}' seri numaralı cihaz sistemde kayıtlı değil.");
            }

            // 3. Bulunan cihazın Id'sini BosDolu kaydına ata
            entity.DollyId = dolly.Id;

            // 4. Zaman damgası (UTC veya yerel saat tercihe göre, DateTime.UtcNow önerilir)
            entity.Time = DateTime.UtcNow;

            try
            {
                // 5. Kaydet
                await _context.BosDolu.AddAsync(entity);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Yeni BosDolu verisi başarıyla eklendi. SerialNumber: {SerialNumber}", entity.SerialNumber);

                return Ok(new
                {
                    Message = "DoluBos Verisi Başarıyla Eklendi",
                    Device = dolly.Name,
                    Status = entity.SensorDegeri == true ? "Dolu" : "Boş"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "BosDolu eklenirken bir veritabanı hatası oluştu. SerialNumber: {SerialNumber}", entity.SerialNumber);
                return StatusCode(500, "Sunucu hatası: " + ex.Message);
            }
        }
    }
}