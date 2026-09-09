using Microsoft.EntityFrameworkCore;
using THYLoggerAPI_POSTGRESQL.Context;
using THYLoggerAPI_POSTGRESQL.Model;

namespace THYLoggerAPI_POSTGRESQL.Services
{
    public class DoluBosService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<DoluBosService> _logger;

        public DoluBosService(ApplicationDbContext context, ILogger<DoluBosService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<BosDolu>> GetAllAsync()
        {
            _logger.LogInformation("Tüm BosDolu verileri listeleniyor.");

            return await _context.BosDolu
                .AsNoTracking()
                .OrderBy(i => i.Id)
                .ToListAsync();
        }

        public async Task<(bool IsSuccess, bool IsNotFound, string? ErrorMessage, object? ResponseData)> AddAsync(BosDolu entity)
        {
            // 1. Seri numarası gönderilmiş mi kontrol et
            if (string.IsNullOrWhiteSpace(entity.SerialNumber))
            {
                _logger.LogWarning("BosDolu ekleme başarısız: SerialNumber gönderilmedi.");
                return (false, false, "SerialNumber gönderilmesi zorunludur.", null);
            }

            // 2. Veritabanında bu seri numarasına sahip Dolly'yi asenkron bul
            var dolly = await _context.Dolly
                .FirstOrDefaultAsync(x => x.SerialNumber == entity.SerialNumber);

            if (dolly == null)
            {
                _logger.LogWarning("'{SerialNumber}' seri numaralı cihaz sistemde kayıtlı değil.", entity.SerialNumber);
                return (false, true, $"'{entity.SerialNumber}' seri numaralı cihaz sistemde kayıtlı değil.", null);
            }

            // 3. Bulunan cihazın Id'sini BosDolu kaydına ata
            entity.DollyId = dolly.Id;

            // 4. Zaman damgası
            entity.Time = DateTime.UtcNow;

            try
            {
                entity.Id = 0; // EF Core otomatize ID üretimi için resetleme

                // 5. Kaydet
                await _context.BosDolu.AddAsync(entity);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Yeni BosDolu verisi başarıyla eklendi. SerialNumber: {SerialNumber}", entity.SerialNumber);

                // Orijinal response yapısı birebir korundu
                var response = new
                {
                    Message = "DoluBos Verisi Başarıyla Eklendi",
                    Device = dolly.Name,
                    Status = entity.SensorDegeri == true ? "Dolu" : "Boş"
                };

                return (true, false, null, response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "BosDolu eklenirken veritabanı hatası oluştu. SerialNumber: {SerialNumber}", entity.SerialNumber);
                throw;
            }
        }
    }
}