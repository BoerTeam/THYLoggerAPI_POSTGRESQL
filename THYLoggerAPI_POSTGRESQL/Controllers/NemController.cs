using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using THYLoggerAPI_POSTGRESQL.Context;
using THYLoggerAPI_POSTGRESQL.Model;

namespace THYLoggerAPI_POSTGRESQL.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NemController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<NemController> _logger;

        public NemController(ApplicationDbContext context, ILogger<NemController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/Nem
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            _logger.LogInformation("Tüm Nem verileri listeleniyor.");

            var list = await _context.Nem
                .AsNoTracking()
                .OrderBy(i => i.Id)
                .ToListAsync();

            return Ok(list);
        }

        // POST: api/Nem
        [HttpPost]
        public async Task<IActionResult> Add([FromBody] Nem entity)
        {
            try
            {
                if (entity == null || string.IsNullOrWhiteSpace(entity.SerialNumber))
                {
                    _logger.LogWarning("SerialNumber zorunludur.");
                    return BadRequest("SerialNumber zorunludur.");
                }

                // 1. Veritabanında ilgili Dolly'yi asenkron ve büyük/küçük harf duyarsız bul
                var dolly = await _context.Dolly
                    .FirstOrDefaultAsync(x => x.SerialNumber.ToLower() == entity.SerialNumber.ToLower());

                if (dolly == null)
                {
                    _logger.LogWarning("Seri numarası {SerialNumber} olan bir Dolly bulunamadı.", entity.SerialNumber);
                    return NotFound($"Seri numarası {entity.SerialNumber} olan bir Dolly bulunamadı.");
                }

                // 2. İlişkileri ata
                entity.DollyId = dolly.Id;

                // 3. Kalibre Edilmiş Nem Hesaplaması (4-20mA -> 0-100% RH Lineer Dönüşüm)
                if (entity.Nem1.HasValue)
                {
                    double rawValue = (double)entity.Nem1.Value; // Cihazdan gelen mA değeri (Örn: 12.35)

                    double inLow = 4.0;
                    double inHigh = 20.0;
                    double outLow = 0.0;   // %0 RH
                    double outHigh = 100.0; // %100 RH

                    // Lineer interpolasyon formülü
                    double hesaplananNem = ((rawValue - inLow) * (outHigh - outLow) / (inHigh - inLow)) + outLow;

                    entity.Nem1 = (float)Math.Round(hesaplananNem, 4);
                }

                // 4. Zaman damgası ve Kayıt
                entity.Time = DateTime.UtcNow;

                await _context.Nem.AddAsync(entity);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Yeni Nem verisi başarıyla eklendi. SerialNumber: {SerialNumber}", entity.SerialNumber);

                return Ok(new
                {
                    Message = "Nem Verisi Başarıyla Eklendi",
                    DollyName = dolly.Name,
                    KaydedilenNem = entity.Nem1,
                    SerialNumber = entity.SerialNumber
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Nem verisi eklenirken bir hata oluştu. SerialNumber: {SerialNumber}", entity?.SerialNumber);
                return StatusCode(500, "Ekleme sırasında bir hata oluştu: " + ex.Message);
            }
        }
    }
}