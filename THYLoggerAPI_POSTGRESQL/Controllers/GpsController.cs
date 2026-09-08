using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

        // GET: api/Gps
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            _logger.LogInformation("Tüm GPS verileri listeleniyor.");

            var list = await _context.Gpsdatum
                .AsNoTracking()
                .OrderBy(i => i.Id)
                .ToListAsync();

            return Ok(list);
        }

        // POST: api/Gps
        [HttpPost]
        public async Task<IActionResult> Add([FromBody] Gpsdatum entity)
        {
            // 1. Seri numarası kontrolü
            if (string.IsNullOrWhiteSpace(entity.SerialNumber))
            {
                _logger.LogWarning("SerialNumber gönderilmesi zorunludur.");
                return BadRequest("SerialNumber gönderilmesi zorunludur.");
            }

            // 2. Bu seri numarasına sahip cihazı (Dolly) asenkron bul
            var dolly = await _context.Dolly
                .FirstOrDefaultAsync(x => x.SerialNumber == entity.SerialNumber);

            if (dolly == null)
            {
                _logger.LogWarning("'{SerialNumber}' seri numaralı cihaz sistemde bulunamadı.", entity.SerialNumber);
                return NotFound($"'{entity.SerialNumber}' seri numaralı cihaz sistemde bulunamadı.");
            }

            // 3. Bulunan cihazın Id'sini GPS verisine ata
            entity.DollyId = dolly.Id;
            entity.Time = DateTime.UtcNow;

            try
            {
                // 4. Kaydet
                await _context.Gpsdatum.AddAsync(entity);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Yeni GPS verisi başarıyla eklendi. SerialNumber: {SerialNumber}", entity.SerialNumber);

                return Ok(new
                {
                    Message = "GPS Verisi Başarıyla Eklendi",
                    DeviceName = dolly.Name,
                    Location = $"{entity.Latitude}, {entity.Longitude}"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GPS verisi eklenirken hata oluştu. SerialNumber: {SerialNumber}", entity.SerialNumber);
                return StatusCode(500, "Sunucu hatası: " + ex.Message);
            }
        }

        // GET: api/Gps/History/5?start=2026-09-01T00:00:00&end=2026-09-09T00:00:00
        [HttpGet("History/{id}")]
        public async Task<IActionResult> GetHistoryData(int id, [FromQuery] DateTime? start, [FromQuery] DateTime? end)
        {
            _logger.LogInformation("GPS geçmiş verileri isteniyor. DollyId: {DollyId}", id);

            var query = _context.Gpsdatum
                .AsNoTracking()
                .Where(x => x.DollyId == id);

            if (start.HasValue) query = query.Where(x => x.Time >= start.Value);
            if (end.HasValue) query = query.Where(x => x.Time <= end.Value);

            try
            {
                var history = await query
                    .OrderBy(x => x.Time)
                    .Select(x => new
                    {
                        lat = x.Latitude,
                        lng = x.Longitude,
                        time = x.Time.HasValue ? x.Time.Value.ToString("dd.MM.yyyy HH:mm:ss") : ""
                    })
                    .ToListAsync();

                _logger.LogInformation("GPS geçmiş verileri başarıyla getirildi. DollyId: {DollyId}, Toplam Kayıt: {Count}", id, history.Count);
                return Ok(history);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GPS geçmiş verileri getirilirken hata oluştu. DollyId: {DollyId}", id);
                return StatusCode(500, "Geçmiş veriler alınırken bir hata oluştu: " + ex.Message);
            }
        }
    }
}