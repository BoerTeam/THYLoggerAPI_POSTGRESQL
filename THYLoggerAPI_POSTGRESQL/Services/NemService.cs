using Microsoft.EntityFrameworkCore;
using THYLoggerAPI_POSTGRESQL.Context;
using THYLoggerAPI_POSTGRESQL.Model;

namespace THYLoggerAPI_POSTGRESQL.Services
{
    public class NemService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<NemService> _logger;

        public NemService(ApplicationDbContext context, ILogger<NemService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<Nem>> GetAllAsync()
        {
            _logger.LogInformation("Tüm Nem verileri listeleniyor.");

            return await _context.Nem
                .AsNoTracking()
                .OrderBy(i => i.Id)
                .ToListAsync();
        }

        public async Task<(bool IsSuccess, bool IsNotFound, string? ErrorMessage, object? ResponseData)> AddAsync(Nem entity)
        {
            // 1. Seri numarası kontrolü
            if (string.IsNullOrWhiteSpace(entity.SerialNumber))
            {
                _logger.LogWarning("SerialNumber zorunludur.");
                return (false, false, "SerialNumber zorunludur.", null);
            }

            // 2. Veritabanında ilgili Dolly'yi asenkron ve büyük/küçük harf duyarsız bul
            var dolly = await _context.Dolly
                .FirstOrDefaultAsync(x => x.SerialNumber.ToLower() == entity.SerialNumber.ToLower());

            if (dolly == null)
            {
                _logger.LogWarning("Seri numarası {SerialNumber} olan bir Dolly bulunamadı.", entity.SerialNumber);
                return (false, true, $"Seri numarası {entity.SerialNumber} olan bir Dolly bulunamadı.", null);
            }

            // 3. İlişkileri ata
            entity.DollyId = dolly.Id;

            // 4. Kalibre Edilmiş Nem Hesaplaması (4-20mA -> 0-100% RH Lineer Dönüşüm)
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

            // 5. Zaman damgası ve Id sıfırlama
            entity.Time = DateTime.UtcNow;
            entity.Id = 0;

            try
            {
                await _context.Nem.AddAsync(entity);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Yeni Nem verisi başarıyla eklendi. SerialNumber: {SerialNumber}", entity.SerialNumber);

                // Orijinal Controller response yapısı birebir korundu
                var response = new
                {
                    Message = "Nem Verisi Başarıyla Eklendi",
                    DollyName = dolly.Name,
                    KaydedilenNem = entity.Nem1,
                    SerialNumber = entity.SerialNumber
                };

                return (true, false, null, response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Nem verisi eklenirken bir hata oluştu. SerialNumber: {SerialNumber}", entity.SerialNumber);
                throw;
            }
        }
    }
}