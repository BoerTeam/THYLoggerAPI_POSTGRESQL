using Microsoft.EntityFrameworkCore;
using THYLoggerAPI_POSTGRESQL.Context;
using THYLoggerAPI_POSTGRESQL.Model;

namespace THYLoggerAPI_POSTGRESQL.Services
{
    public class SicaklikService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<SicaklikService> _logger;

        public SicaklikService(ApplicationDbContext context, ILogger<SicaklikService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<Sicaklik>> GetAllAsync()
        {
            _logger.LogInformation("Tüm Sicaklik verileri listeleniyor.");

            return await _context.Sicaklik
                .AsNoTracking()
                .OrderBy(i => i.Id)
                .ToListAsync();
        }

        public async Task<(bool IsSuccess, bool IsNotFound, string? ErrorMessage, object? ResponseData)> AddAsync(Sicaklik entity)
        {
            // 1. Seri numarası kontrolü
            if (entity == null || string.IsNullOrWhiteSpace(entity.SerialNumber))
            {
                _logger.LogWarning("SerialNumber gönderilmesi zorunludur.");
                return (false, false, "SerialNumber (Seri Numarası) gönderilmesi zorunludur.", null);
            }

            try
            {
                // 2. Veritabanında ilgili Dolly'yi asenkron ve büyük/küçük harf duyarsız bul
                var dolly = await _context.Dolly
                    .FirstOrDefaultAsync(x => x.SerialNumber.ToLower() == entity.SerialNumber.ToLower());

                if (dolly == null)
                {
                    _logger.LogWarning("'{SerialNumber}' seri numarasına sahip bir cihaz sistemde kayıtlı değil.", entity.SerialNumber);
                    return (false, true, $"'{entity.SerialNumber}' seri numarasına sahip bir cihaz sistemde kayıtlı değil.", null);
                }

                // 3. İlişki ve ID düzenlemeleri
                entity.DollyId = dolly.Id;
                entity.Id = 0; // EF Core otomatik ID üretimi için resetleme

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

                // Orijinal yanıt formatı birebir korundu
                var response = new
                {
                    Status = "Başarılı",
                    Message = "Sıcaklık Verisi Eklendi",
                    CalculatedValue = entity.Sicaklik1,
                    Device = dolly.Name
                };

                return (true, false, null, response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Sıcaklık verisi eklenirken bir hata oluştu. SerialNumber: {SerialNumber}", entity?.SerialNumber);
                throw;
            }
        }
    }
}