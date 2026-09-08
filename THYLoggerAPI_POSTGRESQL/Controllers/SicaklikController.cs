using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using THYLoggerAPI_POSTGRESQL.Context;
using THYLoggerAPI_POSTGRESQL.Model;

namespace THYLoggerAPI_POSTGRESQL.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SicaklikController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<SicaklikController> _logger;

        public SicaklikController(ApplicationDbContext context, ILogger<SicaklikController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/Sicaklik
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            _logger.LogInformation("Tüm Sicaklik verileri listeleniyor.");

            var list = await _context.Sicaklik
                .AsNoTracking()
                .OrderBy(i => i.Id)
                .ToListAsync();

            return Ok(list);
        }

        // POST: api/Sicaklik
        [HttpPost]
        public async Task<IActionResult> Add([FromBody] Sicaklik entity)
        {
            // 1. Seri numarası kontrolü
            if (entity == null || string.IsNullOrWhiteSpace(entity.SerialNumber))
            {
                _logger.LogWarning("SerialNumber gönderilmesi zorunludur.");
                return BadRequest("SerialNumber (Seri Numarası) gönderilmesi zorunludur.");
            }

            try
            {
                // 2. Veritabanında ilgili Dolly'yi asenkron ve büyük/küçük harf duyarsız bul
                var dolly = await _context.Dolly
                    .FirstOrDefaultAsync(x => x.SerialNumber.ToLower() == entity.SerialNumber.ToLower());

                if (dolly == null)
                {
                    _logger.LogWarning("'{SerialNumber}' seri numarasına sahip bir cihaz sistemde kayıtlı değil.", entity.SerialNumber);
                    return NotFound($"'{entity.SerialNumber}' seri numarasına sahip bir cihaz sistemde kayıtlı değil.");
                }

                // 3. İlişkileri ata
                entity.DollyId = dolly.Id;

                // 4. Kalibre Edilmiş Sıcaklık Hesaplaması (4-20mA -> 0-100°C Lineer Dönüşüm)
                if (entity.Sicaklik1.HasValue)
                {
                    double rawValue = (double)entity.Sicaklik1.Value; // Cihazdan gelen mA değeri (Örn: 9.92)

                    double inLow = 4.0;
                    double inHigh = 20.0;
                    double outLow = 0.0;   // 0°C
                    double outHigh = 100.0; // 100°C

                    // Lineer interpolasyon formülü
                    double hesaplanan = ((rawValue - inLow) * (outHigh - outLow) / (inHigh - inLow)) + outLow;

                    entity.Sicaklik1 = (float)Math.Round(hesaplanan, 2);
                }

                // 5. Zaman damgası ve Kayıt
                entity.Time = DateTime.UtcNow;

                await _context.Sicaklik.AddAsync(entity);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Yeni Sicaklik verisi başarıyla eklendi. SerialNumber: {SerialNumber}", entity.SerialNumber);

                return Ok(new
                {
                    Status = "Başarılı",
                    Message = "Sıcaklık Verisi Eklendi",
                    CalculatedValue = entity.Sicaklik1,
                    Device = dolly.Name
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Sıcaklık verisi eklenirken bir hata oluştu. SerialNumber: {SerialNumber}", entity?.SerialNumber);
                return StatusCode(500, "Ekleme sırasında bir hata oluştu: " + ex.Message);
            }
        }
    }
}