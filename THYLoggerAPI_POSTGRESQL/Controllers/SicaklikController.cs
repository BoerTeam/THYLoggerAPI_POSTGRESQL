using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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
        [HttpGet]
        public IEnumerable<Sicaklik> Get()
        {
            _logger.LogInformation("Tüm Sicaklik verileri listeleniyor.");
            return _context.Sicaklik.OrderBy(i => i.Id).ToList();            
        }

        [HttpPost("Add")]
        public IActionResult Add(Sicaklik entity)
        {
            // 1. Seri numarası kontrolü
            if (string.IsNullOrEmpty(entity.SerialNumber))
            {
                return BadRequest("SerialNumber (Seri Numarası) gönderilmesi zorunludur.");
            }

            var dolly = _context.Dolly.FirstOrDefault(x => x.SerialNumber == entity.SerialNumber);

            if (dolly == null)
            {
                _logger.LogError("'{SerialNumber}' seri numarasına sahip bir cihaz sistemde kayıtlı değil.", entity.SerialNumber);
                return NotFound($"'{entity.SerialNumber}' seri numarasına sahip bir cihaz sistemde kayıtlı değil.");
            }

            entity.DollyId = dolly.Id;

            if (entity.Sicaklik1.HasValue)
            {
                double rawValue = (double)entity.Sicaklik1.Value; // Cihazdan gelen mA değeri (Örn: 9.92)

                // Excel tablosundaki yeni skala değerleri
                double inLow = 4.0;
                double inHigh = 20.0;
                double outLow = 0.0;   // scl: 0
                double outHigh = 100.0; // sch: 100

                // Lineer interpolasyon formülü
                double hesaplanan = ((rawValue - inLow) * (outHigh - outLow) / (inHigh - inLow)) + outLow;

                // Görseldeki örneğe göre: ((9.92 - 4) * (100 - 0) / (20 - 4)) + 0 = (5.92 * 100 / 16) = 37
                entity.Sicaklik1 = (float)Math.Round(hesaplanan, 2);
            }

            entity.Time = DateTime.Now;

            _context.Sicaklik.Add(entity);
            _context.SaveChanges();
            _logger.LogInformation("Yeni Sicaklik verisi başarıyla eklendi. SerialNumber: {SerialNumber}", entity.SerialNumber);
            return Ok(new
            {
                Status = "Başarılı",
                Message = "Sıcaklık Verisi Eklendi",
                CalculatedValue = entity.Sicaklik1,
                Device = dolly.Name
            });
        }
    }
}
