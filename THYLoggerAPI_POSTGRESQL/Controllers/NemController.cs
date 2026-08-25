using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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
        [HttpGet("Get")]
        public IEnumerable<Nem> Get()
        {
            _logger.LogInformation("Tüm Nem verileri listeleniyor.");
                return _context.Nem.OrderBy(i => i.Id).ToList();
            
        }

        [HttpPost("Add")]
        public IActionResult Add([FromBody] Nem entity)
        {
            // 1. Veri kontrolü
            if (entity == null || string.IsNullOrEmpty(entity.SerialNumber))
            {
                _logger.LogError("SerialNumber zorunludur.");
                return BadRequest("SerialNumber zorunludur.");
            }

            // 2. Veritabanında ilgili Dolly'yi bul
            // (Büyük/Küçük harf duyarlılığını kaldırmak için ToLower() kullanıldı)
            var dolly = _context.Dolly.FirstOrDefault(x => x.SerialNumber.ToLower() == entity.SerialNumber.ToLower());

            if (dolly == null)
            {
                _logger.LogError("Seri numarası {SerialNumber} olan bir Dolly bulunamadı.", entity.SerialNumber);
                return NotFound($"Seri numarası {entity.SerialNumber} olan bir Dolly bulunamadı.");
            }

            // 3. İlişkileri ata
            entity.DollyId = dolly.Id;

            // 4. Kalibre Edilmiş Nem Hesaplaması
            if (entity.Nem1.HasValue)
            {
                double rawValue = (double)entity.Nem1.Value; // Cihazdan gelen mA değeri (Örn: 12.35)

                // Excel tablosundaki güncel skala değerleri
                double inLow = 4.0;
                double inHigh = 20.0;
                double outLow = 0.0;   // scl: 0
                double outHigh = 100.0; // sch: 100

                // Lineer interpolasyon formülü
                double hesaplananNem = ((rawValue - inLow) * (outHigh - outLow) / (inHigh - inLow)) + outLow;

                // Tablodaki 52,1875 değerini tam yakalamak için virgülden sonra 4 basamağa yuvarlıyoruz
                entity.Nem1 = (float)Math.Round(hesaplananNem, 4);
            }

            // 5. Zaman ve Kayıt
            entity.Time = DateTime.Now;

            _context.Nem.Add(entity);
            _context.SaveChanges();
            
            _logger.LogInformation("Yeni Nem verisi başarıyla eklendi. SerialNumber: {SerialNumber}", entity.SerialNumber);

            // Yanıt dönerken kaydedilen veriyi de gösteriyoruz
            return Ok(new
            {
                Message = "Nem Verisi Başarıyla Eklendi",
                DollyName = dolly.Name,
                KaydedilenNem = entity.Nem1,
                SerialNumber = entity.SerialNumber
            });
        }
    }
}
