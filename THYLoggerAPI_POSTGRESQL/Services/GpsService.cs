using Microsoft.EntityFrameworkCore;
using THYLoggerAPI_POSTGRESQL.Context;
using THYLoggerAPI_POSTGRESQL.Model;

namespace THYLoggerAPI_POSTGRESQL.Services
{
    public class GpsService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<GpsService> _logger;

        public GpsService(ApplicationDbContext context, ILogger<GpsService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<Gpsdatum>> GetAllAsync()
        {
            _logger.LogInformation("Tüm GPS verileri listeleniyor.");

            return await _context.Gpsdatum
                .AsNoTracking()
                .OrderBy(i => i.Id)
                .ToListAsync();
        }

        public async Task<(bool IsSuccess, bool IsNotFound, string? ErrorMessage, object? ResponseData)> AddAsync(Gpsdatum entity)
        {
            // 1. Seri numarası kontrolü
            if (string.IsNullOrWhiteSpace(entity.SerialNumber))
            {
                _logger.LogWarning("GPS verisi kaydı başarısız: SerialNumber gönderilmedi.");
                return (false, false, "SerialNumber gönderilmesi zorunludur.", null);
            }

            // 2. Bu seri numarasına sahip cihazı (Dolly) bul
            var dolly = await _context.Dolly
                .FirstOrDefaultAsync(x => x.SerialNumber == entity.SerialNumber);

            if (dolly == null)
            {
                _logger.LogWarning("'{SerialNumber}' seri numaralı cihaz sistemde bulunamadı.", entity.SerialNumber);
                return (false, true, $"'{entity.SerialNumber}' seri numaralı cihaz sistemde bulunamadı.", null);
            }

            // 3. Cihaz ID'si ve veritabanı kayıt zamanı ataması
            entity.DollyId = dolly.Id;
            entity.Time = DateTime.UtcNow; // Cihazdan gelen 1970 varsayılan zaman yerine sunucu zamanı basılır

            try
            {
                // Cihaz Id:0 gönderdiği için Entity Framework'ün otomatik ID vermesini sağlıyoruz
                entity.Id = 0;

                await _context.Gpsdatum.AddAsync(entity);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Yeni GPS verisi başarıyla eklendi. SerialNumber: {SerialNumber}, FixAvailable: {FixAvailable}",
                    entity.SerialNumber, entity.GpsFixAvailable);

                var response = new
                {
                    Message = "GPS Verisi Başarıyla Eklendi",
                    DeviceName = dolly.Name,
                    Location = $"{entity.Latitude}, {entity.Longitude}"
                };

                return (true, false, null, response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GPS verisi eklenirken hata oluştu. SerialNumber: {SerialNumber}", entity.SerialNumber);
                throw;
            }
        }

        public async Task<object> GetHistoryDataAsync(int id, DateTime? start, DateTime? end)
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
                return history;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GPS geçmiş verileri getirilirken hata oluştu. DollyId: {DollyId}", id);
                throw;
            }
        }
    }
}