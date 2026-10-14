using System.Collections.Concurrent;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using THYLoggerAPI_POSTGRESQL.Context;
using THYLoggerAPI_POSTGRESQL.Model;

namespace THYLoggerAPI_POSTGRESQL.Services
{
    public class DoluBosService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<DoluBosService> _logger;
        private readonly DoluBosDebounceOptions _debounceOptions;

        // Dolly basina, mevcut durumdan farkli gelen okumalarin art arda sayisi (servis instance'lari arasi paylasilir)
        private static readonly ConcurrentDictionary<int, (bool Candidate, int Count)> _pending = new();

        public DoluBosService(ApplicationDbContext context, ILogger<DoluBosService> logger, IOptions<DoluBosDebounceOptions> debounceOptions)
        {
            _context = context;
            _logger = logger;
            _debounceOptions = debounceOptions.Value;
        }

        public async Task<List<BosDolu>> GetAllAsync(int? dollyId = null, DateTime? start = null, DateTime? end = null)
        {
            _logger.LogInformation("Tüm BosDolu verileri listeleniyor.");

            var query = _context.BosDolu.AsNoTracking().AsQueryable();
            if (dollyId.HasValue) query = query.Where(x => x.DollyId == dollyId.Value);
            if (start.HasValue) query = query.Where(x => x.Time >= start.Value);
            if (end.HasValue) query = query.Where(x => x.Time <= end.Value);

            return await query.OrderBy(i => i.Id).ToListAsync();
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

            // 3.1 Debounce: durum degisimi ancak art arda N okumada dogrulanirsa kaydedilir
            var required = Math.Max(1, _debounceOptions.RequiredConsecutive);
            if (required > 1 && entity.SensorDegeri.HasValue)
            {
                var incoming = entity.SensorDegeri.Value;
                var lastState = await _context.BosDolu
                    .AsNoTracking()
                    .Where(x => x.DollyId == dolly.Id && x.SensorDegeri != null)
                    .OrderByDescending(x => x.Time)
                    .Select(x => x.SensorDegeri)
                    .FirstOrDefaultAsync();

                if (lastState.HasValue && lastState.Value != incoming)
                {
                    var count = _pending.AddOrUpdate(
                        dolly.Id,
                        _ => (incoming, 1),
                        (_, p) => p.Candidate == incoming ? (incoming, p.Count + 1) : (incoming, 1)).Count;

                    if (count < required)
                    {
                        _logger.LogInformation("BosDolu degisimi beklemede ({Count}/{Required}). SerialNumber: {SerialNumber}", count, required, entity.SerialNumber);
                        return (true, false, null, new
                        {
                            Message = "DoluBos degisimi dogrulama bekliyor",
                            Device = dolly.Name,
                            Status = lastState.Value ? "Dolu" : "Bo\u015f",
                            Pending = $"{count}/{required}"
                        });
                    }
                }

                // Durum ayni kaldi ya da degisim dogrulandi: bekleyen sayaci sifirla
                _pending.TryRemove(dolly.Id, out _);
            }

            // 4. Zaman damgası
            // Windows ve Linux (IIS/Docker) uyumlu Türkiye saati alımı:
            var turkeyTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Turkey Standard Time");
            entity.Time = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, turkeyTimeZone);

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