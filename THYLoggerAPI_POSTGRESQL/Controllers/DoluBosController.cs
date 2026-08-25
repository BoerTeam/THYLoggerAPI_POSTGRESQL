using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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
        [HttpGet("Get")]
        public IEnumerable<BosDolu> Get()
        {
            _logger.LogInformation("Tüm Dolly verileri listeleniyor.");
            return _context.BosDolu.OrderBy(i => i.Id).ToList();
            
        }

        [HttpPost("Add")]
        public IActionResult Add(BosDolu entity)
        {
            // 1. Seri numarası gönderilmiş mi kontrol et
            if (string.IsNullOrEmpty(entity.SerialNumber))
            {
                _logger.LogError("SerialNumber gönderilmesi zorunludur.");
                return BadRequest("SerialNumber gönderilmesi zorunludur.");
            }

            // 2. Veritabanında bu seri numarasına sahip Dolly'yi bul
            var dolly = _context.Dolly.FirstOrDefault(x => x.SerialNumber == entity.SerialNumber);

            if (dolly == null)
            {
                _logger.LogError("'{SerialNumber}' seri numaralı cihaz sistemde kayıtlı değil.", entity.SerialNumber);
                return NotFound($"'{entity.SerialNumber}' seri numaralı cihaz sistemde kayıtlı değil.");
            }

            // 3. Bulunan cihazın Id'sini BosDolu kaydına ata
            entity.DollyId = dolly.Id;

            // 4. Zaman damgası kontrolü

            entity.Time = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc);


            // 5. Kaydet
            _context.BosDolu.Add(entity);
            _context.SaveChanges();
            _logger.LogInformation("Yeni DoluBos verisi başarıyla eklendi. SerialNumber: {SerialNumber}", entity.SerialNumber);

            return Ok(new
            {
                Message = "DoluBos Verisi Başarıyla Eklendi",
                Device = dolly.Name,
                Status = entity.SensorDegeri == true ? "Dolu" : "Boş"
            });
        }
    }
}
