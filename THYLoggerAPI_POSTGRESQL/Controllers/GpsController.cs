using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using THYLoggerAPI_POSTGRESQL.Context;
using THYLoggerAPI_POSTGRESQL.Model;

namespace THYLoggerAPI_POSTGRESQL.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GpsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<GpsController> _logger;
        public GpsController(ApplicationDbContext context, ILogger<GpsController> logger)
        {
            _context = context;
            _logger = logger;
        }
        [HttpGet("Get")]
        public IEnumerable<Gpsdatum> Get()
        {
            _logger.LogInformation("Tüm Gpsdatum verileri listeleniyor.");
            return _context.Gpsdatum.OrderBy(i => i.Id).ToList();
        }

        [HttpPost("Add")]
        public IActionResult Add(Gpsdatum entity)
        {
            // 1. Seri numarası gönderilmiş mi?
            if (string.IsNullOrEmpty(entity.SerialNumber))
            {
                _logger.LogError("SerialNumber gönderilmesi zorunludur.");
                return BadRequest("SerialNumber gönderilmesi zorunludur.");
            }

            // 2. Bu seri numarasına sahip cihazı (Dolly) veritabanında bul
            var dolly = _context.Dolly.FirstOrDefault(x => x.SerialNumber == entity.SerialNumber);

            if (dolly == null)
            {
                _logger.LogError("'{SerialNumber}' seri numaralı cihaz sistemde bulunamadı.", entity.SerialNumber);
                return NotFound($"'{entity.SerialNumber}' seri numaralı cihaz sistemde bulunamadı.");
            }

            // 3. Bulunan cihazın Id'sini GPS verisine ata
            entity.DollyId = dolly.Id;

            
             entity.Time = DateTime.UtcNow;
            

            // 5. Kaydet
            _context.Gpsdatum.Add(entity);
            _context.SaveChanges();
           _logger.LogInformation("Yeni GPS verisi başarıyla eklendi. SerialNumber: {SerialNumber}", entity.SerialNumber);
            return Ok(new
            {
                Message = "GPS Verisi Başarıyla Eklendi",
                DeviceName = dolly.Name,
                Location = $"{entity.Latitude}, {entity.Longitude}"
            });
        }
        [HttpGet("GetHistoryData")]
        public IActionResult GetHistoryData(int id, DateTime? start, DateTime? end)
        {
            var query = _context.Gpsdatum.Where(x => x.DollyId == id);

            if (start.HasValue) query = query.Where(x => x.Time >= start.Value);
            if (end.HasValue) query = query.Where(x => x.Time <= end.Value);

            var history = query
                .OrderBy(x => x.Time)
                .Select(x => new {
                    lat = x.Latitude,
                    lng = x.Longitude,
                    time = x.Time.HasValue ? x.Time.Value.ToString("dd.MM.yyyy HH:mm:ss") : ""
                })
                .ToList();

            _logger.LogInformation("GPS geçmiş verileri başarıyla getirildi. DollyId: {DollyId}", id);
            return Ok(history); // RestSharp başarılı sayılması için Ok(200) dönmeli
        }
    }
}
