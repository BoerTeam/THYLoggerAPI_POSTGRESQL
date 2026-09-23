using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using THYLoggerAPI_POSTGRESQL.Context;
using THYLoggerAPI_POSTGRESQL.Model;

namespace THYLoggerAPI_POSTGRESQL.Services
{
    public class NemService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<NemService> _logger;
        private readonly NemCalibrationOptions _calibrationOptions;

        public NemService(
            ApplicationDbContext context,
            ILogger<NemService> logger,
            IOptions<NemCalibrationOptions> calibrationOptions)
        {
            _context = context;
            _logger = logger;
            _calibrationOptions = calibrationOptions.Value; // appsettings.json verileri
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

            // 4. Kalibre Edilmiş Nem Hesaplaması (Dışarıdan appsettings.json Üzerinden)
            if (entity.Nem1.HasValue)
            {
                double rawValue = (double)entity.Nem1.Value; // Cihazdan gelen mA değeri (Örn: 12.35)

                // appsettings.json -> SensorCalibration:Nem alanından gelen değerler
                double inLow = _calibrationOptions.InLow;
                double inHigh = _calibrationOptions.InHigh;
                double outLow = _calibrationOptions.OutLow;
                double outHigh = _calibrationOptions.OutHigh;

                // Lineer interpolasyon formülü
                double hesaplananNem = ((rawValue - inLow) * (outHigh - outLow) / (inHigh - inLow)) + outLow;

                entity.Nem1 = (float)Math.Round(hesaplananNem, _calibrationOptions.Precision);
            }

            // 5. Zaman damgası ve Id sıfırlama
            // Windows ve Linux (IIS/Docker) uyumlu Türkiye saati alımı:
            var turkeyTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Turkey Standard Time");
            entity.Time = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, turkeyTimeZone);
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