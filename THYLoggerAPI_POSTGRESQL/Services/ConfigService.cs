using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using THYLoggerAPI_POSTGRESQL.Context;
using THYLoggerAPI_POSTGRESQL.DTOs;
using THYLoggerAPI_POSTGRESQL.Model;

namespace THYLoggerAPI_POSTGRESQL.Services
{
    public class ConfigService
    {
        public const long MaxLogFileBytes = 20 * 1024 * 1024;

        private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

        private readonly ApplicationDbContext _context;
        private readonly ILogger<ConfigService> _logger;
        private readonly IWebHostEnvironment _env;

        public ConfigService(ApplicationDbContext context, ILogger<ConfigService> logger, IWebHostEnvironment env)
        {
            _context = context;
            _logger = logger;
            _env = env;
        }

        private static DateTime TurkeyNow() =>
            TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Turkey Standard Time"));

        // Cihaz "Z" (UTC) ile zaman gönderir; diğer tüm zaman damgaları Türkiye saatinde tutulduğu için çevrilir
        private static DateTime ToTurkeyTime(DateTime value)
        {
            var tz = TimeZoneInfo.FindSystemTimeZoneById("Turkey Standard Time");
            return value.Kind switch
            {
                DateTimeKind.Utc => TimeZoneInfo.ConvertTimeFromUtc(value, tz),
                DateTimeKind.Local => TimeZoneInfo.ConvertTimeFromUtc(value.ToUniversalTime(), tz),
                _ => value
            };
        }

        private Task<Dolly?> FindDollyAsync(string serialNumber) =>
            _context.Dolly.FirstOrDefaultAsync(x => x.SerialNumber.ToLower() == serialNumber.ToLower());

        private void AddEvent(int dollyId, string type, int? version, string? message, DateTime at) =>
            _context.DollyConfigEvents.Add(new DollyConfigEvent
            {
                DollyId = dollyId,
                EventType = type,
                ConfigVersion = version,
                Message = message,
                CreatedAt = at
            });

        // ==========================================
        // CİHAZ TARAFI
        // ==========================================

        /// <summary>Cihazın config'ini döner; cihazdaki sürüm güncel ise config null gider.</summary>
        public async Task<(bool IsSuccess, bool IsNotFound, string? ErrorMessage, ConfigGetResponse? Response)> GetAsync(ConfigGetRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.SerialNumber))
                return (false, false, "SerialNumber zorunludur.", null);
            if (request.ConfigVersion == null)
                return (false, false, "ConfigVersion zorunludur.", null);

            var dolly = await FindDollyAsync(request.SerialNumber);
            if (dolly == null)
                return (false, true, $"Seri numarası {request.SerialNumber} olan bir Dolly bulunamadı.", null);

            var now = TurkeyNow();
            var setting = await _context.DollySettings.FirstOrDefaultAsync(x => x.DollyId == dolly.Id);
            if (setting == null)
            {
                // İlk bağlanışta varsayılan config oluşturulur (cihaz 0 ile gelir, 1'i alır)
                setting = new DollySetting
                {
                    DollyId = dolly.Id,
                    ConfigVersion = 1,
                    ConfigJson = JsonSerializer.Serialize(new DeviceConfig(), JsonOptions)
                };
                _context.DollySettings.Add(setting);
            }

            setting.DeviceType = request.DeviceType;
            setting.AppVersion = request.AppVersion;
            setting.LastSeenAt = now;
            setting.UpdatedAt = now;

            var hasNewConfig = request.ConfigVersion.Value != setting.ConfigVersion;
            if (hasNewConfig)
            {
                // Aynı sürümü cihaz her poll'da tekrar çekebilir; olay yalnızca sürüm değişince yazılır
                if (setting.FetchedConfigVersion != setting.ConfigVersion)
                    AddEvent(dolly.Id, DollyConfigEventTypes.Fetched, setting.ConfigVersion,
                        $"Cihaz sürüm {request.ConfigVersion} iken {setting.ConfigVersion} sürümünü çekti.", now);

                setting.FetchedConfigVersion = setting.ConfigVersion;
                setting.FetchedAt = now;
            }

            await _context.SaveChangesAsync();

            if (!hasNewConfig)
            {
                return (true, false, null, new ConfigGetResponse
                {
                    Message = "Değişiklik yok",
                    ConfigVersion = setting.ConfigVersion,
                    Config = null
                });
            }

            return (true, false, null, new ConfigGetResponse
            {
                Message = "Konfigürasyon gönderildi",
                ConfigVersion = setting.ConfigVersion,
                Config = JsonSerializer.Deserialize<DeviceConfig>(setting.ConfigJson, JsonOptions)
            });
        }

        /// <summary>Cihaz, config'i uyguladığını (veya uygulayamadığını) bildirir.</summary>
        public async Task<(bool IsSuccess, bool IsNotFound, string? ErrorMessage)> VsControlAsync(ConfigVsControlRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.SerialNumber))
                return (false, false, "SerialNumber zorunludur.");
            if (request.ConfigVersion == null)
                return (false, false, "ConfigVersion zorunludur.");
            if (request.AppliedAt == null)
                return (false, false, "AppliedAt zorunludur.");

            var dolly = await FindDollyAsync(request.SerialNumber);
            if (dolly == null)
                return (false, true, $"Seri numarası {request.SerialNumber} olan bir Dolly bulunamadı.");

            var setting = await _context.DollySettings.FirstOrDefaultAsync(x => x.DollyId == dolly.Id);
            if (setting == null)
                return (false, true, $"{request.SerialNumber} için konfigürasyon kaydı bulunamadı.");

            var now = TurkeyNow();
            var appliedAt = ToTurkeyTime(request.AppliedAt.Value);
            setting.LastSeenAt = now;
            setting.UpdatedAt = now;

            if (request.Success == false)
            {
                var message = string.IsNullOrWhiteSpace(request.ErrorMessage) ? "Cihaz hata bildirdi." : request.ErrorMessage.Trim();
                if (message.Length > 1000) message = message[..1000];

                setting.LastError = message;
                setting.LastErrorAt = appliedAt;
                AddEvent(dolly.Id, DollyConfigEventTypes.Error, request.ConfigVersion, message, appliedAt);
            }
            else
            {
                setting.AppliedConfigVersion = request.ConfigVersion;
                setting.AppliedAt = appliedAt;
                // Başarılı uygulama önceki hatayı geçersiz kılar
                setting.LastError = null;
                setting.LastErrorAt = null;
                AddEvent(dolly.Id, DollyConfigEventTypes.Applied, request.ConfigVersion, "Cihaz konfigürasyonu uyguladı.", appliedAt);
            }

            await _context.SaveChangesAsync();
            return (true, false, null);
        }

        /// <summary>Cihazın gönderdiği log dosyasını Logs/DollyLogs/{seriNo}/ altına kaydeder.</summary>
        public async Task<(bool IsSuccess, bool IsNotFound, string? ErrorMessage, string? FileName)> SaveLogAsync(IFormFile? logFile, string? serialNumber)
        {
            if (string.IsNullOrWhiteSpace(serialNumber))
                return (false, false, "SerialNumber zorunludur.", null);
            if (logFile == null || logFile.Length == 0)
                return (false, false, "LogFile zorunludur.", null);
            if (logFile.Length > MaxLogFileBytes)
                return (false, false, "LogFile en fazla 20 MB olabilir.", null);

            var dolly = await FindDollyAsync(serialNumber);
            if (dolly == null)
                return (false, true, $"Seri numarası {serialNumber} olan bir Dolly bulunamadı.", null);

            // Klasör/dosya adı kullanıcı girdisinden geldiği için path traversal'a karşı temizlenir
            var safeSerial = Path.GetFileName(dolly.SerialNumber);
            var safeName = Path.GetFileName(logFile.FileName);
            var now = TurkeyNow();
            var fileName = $"{now:yyyyMMdd_HHmmss}_{(string.IsNullOrWhiteSpace(safeName) ? "log.txt" : safeName)}";

            var dir = Path.Combine(_env.ContentRootPath, "Logs", "DollyLogs", safeSerial);
            Directory.CreateDirectory(dir);

            await using (var stream = File.Create(Path.Combine(dir, fileName)))
            {
                await logFile.CopyToAsync(stream);
            }

            AddEvent(dolly.Id, DollyConfigEventTypes.LogUploaded, null, fileName, now);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Dolly log dosyası kaydedildi. SerialNumber: {SerialNumber}, Dosya: {FileName}", serialNumber, fileName);
            return (true, false, null, fileName);
        }

        // ==========================================
        // DASHBOARD TARAFI
        // ==========================================

        public async Task<ConfigDeviceListResponse> GetDevicesAsync()
        {
            var rows = await _context.Dolly.AsNoTracking()
                .OrderBy(d => d.Name)
                .GroupJoin(_context.DollySettings.AsNoTracking(), d => d.Id, s => s.DollyId, (d, s) => new { d, s })
                .SelectMany(x => x.s.DefaultIfEmpty(), (x, s) => new { x.d, s })
                .ToListAsync();

            var items = rows.Select(r => new ConfigDeviceItem
            {
                DollyId = r.d.Id,
                Name = r.d.Name,
                SerialNumber = r.d.SerialNumber,
                IsActive = r.d.IsActive,
                DeviceType = r.s?.DeviceType,
                AppVersion = r.s?.AppVersion,
                ServerConfigVersion = r.s?.ConfigVersion,
                FetchedConfigVersion = r.s?.FetchedConfigVersion,
                FetchedAt = r.s?.FetchedAt,
                AppliedConfigVersion = r.s?.AppliedConfigVersion,
                AppliedAt = r.s?.AppliedAt,
                LastSeenAt = r.s?.LastSeenAt,
                LastError = r.s?.LastError,
                LastErrorAt = r.s?.LastErrorAt,
                Status = ResolveStatus(r.s)
            }).ToList();

            return new ConfigDeviceListResponse
            {
                Items = items,
                DeviceTypes = items.Where(i => !string.IsNullOrWhiteSpace(i.DeviceType))
                    .Select(i => i.DeviceType!).Distinct().OrderBy(x => x).ToList()
            };
        }

        private static string ResolveStatus(DollySetting? s)
        {
            if (s == null) return "NoConfig";
            if (s.LastError != null) return "Error";
            if (s.AppliedConfigVersion == s.ConfigVersion) return "Applied";
            if (s.FetchedConfigVersion == s.ConfigVersion) return "Fetched";
            return s.LastSeenAt == null ? "NeverSeen" : "Pending";
        }

        /// <summary>Seçili cihazlara (ve/veya modele) yeni config yayınlar; her cihazın ConfigVersion değeri 1 artar.</summary>
        public async Task<(bool IsSuccess, string? ErrorMessage, ConfigPublishResponse? Response)> PublishAsync(ConfigPublishRequest request)
        {
            if (request.Config == null)
                return (false, "Config zorunludur.", null);

            var validation = Validate(request.Config);
            if (validation != null)
                return (false, validation, null);

            var targetIds = new HashSet<int>(request.DollyIds ?? new List<int>());
            if (!string.IsNullOrWhiteSpace(request.DeviceType))
            {
                var byType = await _context.DollySettings.AsNoTracking()
                    .Where(s => s.DeviceType == request.DeviceType)
                    .Select(s => s.DollyId)
                    .ToListAsync();
                targetIds.UnionWith(byType);
            }

            if (targetIds.Count == 0)
                return (false, "Hedef cihaz bulunamadı. En az bir cihaz veya cihaz modeli seçin.", null);

            var dollies = await _context.Dolly.Where(d => targetIds.Contains(d.Id)).ToListAsync();
            if (dollies.Count == 0)
                return (false, "Seçilen cihazlar bulunamadı.", null);

            var settings = await _context.DollySettings
                .Where(s => targetIds.Contains(s.DollyId))
                .ToDictionaryAsync(s => s.DollyId);

            var configJson = JsonSerializer.Serialize(request.Config, JsonOptions);
            var now = TurkeyNow();
            var response = new ConfigPublishResponse();

            foreach (var dolly in dollies)
            {
                if (!settings.TryGetValue(dolly.Id, out var setting))
                {
                    setting = new DollySetting { DollyId = dolly.Id, ConfigVersion = 0 };
                    _context.DollySettings.Add(setting);
                }

                setting.ConfigVersion++;
                setting.ConfigJson = configJson;
                setting.UpdatedAt = now;
                // Yeni sürüm için önceki hata geçerliliğini yitirir
                setting.LastError = null;
                setting.LastErrorAt = null;

                var message = string.IsNullOrWhiteSpace(request.Note) ? "Yeni konfigürasyon yayınlandı." : $"Yeni konfigürasyon yayınlandı: {request.Note.Trim()}";
                AddEvent(dolly.Id, DollyConfigEventTypes.Published, setting.ConfigVersion, message, now);

                response.Items.Add(new ConfigPublishItem
                {
                    DollyId = dolly.Id,
                    SerialNumber = dolly.SerialNumber,
                    ConfigVersion = setting.ConfigVersion
                });
            }

            await _context.SaveChangesAsync();
            response.UpdatedCount = response.Items.Count;

            _logger.LogInformation("Konfigürasyon {Count} cihaza yayınlandı.", response.UpdatedCount);
            return (true, null, response);
        }

        private static string? Validate(DeviceConfig c)
        {
            if (c.ConfigPollIntervalSec < 1) return "configPollIntervalSec en az 1 olmalıdır.";
            if (c.ReadIntervalSec < 1) return "readIntervalSec en az 1 olmalıdır.";
            if (c.SendIntervalSec < 1) return "sendIntervalSec en az 1 olmalıdır.";
            if (c.GpsIntervalSec < 1) return "gpsIntervalSec en az 1 olmalıdır.";
            if (c.GpsNoFixLogIntervalSec < 1) return "gpsNoFixLogIntervalSec en az 1 olmalıdır.";

            foreach (var ch in c.AnalogChannels)
            {
                if (string.IsNullOrWhiteSpace(ch.Channel)) return "Analog kanal adı boş olamaz.";
                if (string.IsNullOrWhiteSpace(ch.Unit)) return $"{ch.Channel} için birim boş olamaz.";
                if (ch.Send && string.IsNullOrWhiteSpace(ch.Measurement)) return $"{ch.Channel} gönderilecekse measurement zorunludur.";
            }

            var endpoints = new[] { c.Endpoints.Nem, c.Endpoints.Sicaklik, c.Endpoints.Gps, c.Endpoints.DoluBos };
            if (endpoints.Any(e => string.IsNullOrWhiteSpace(e) || !e.StartsWith('/')))
                return "Endpoint değerleri '/' ile başlamalıdır.";

            return null;
        }

        public async Task<ConfigEventPageResponse> GetEventsAsync(
            int? dollyId, string? eventType, DateTime? start, DateTime? end, int page, int pageSize)
        {
            page = Math.Max(1, page);
            pageSize = Math.Clamp(pageSize, 10, 200);

            var query = _context.DollyConfigEvents.AsNoTracking().AsQueryable();
            if (dollyId.HasValue) query = query.Where(e => e.DollyId == dollyId.Value);
            if (!string.IsNullOrWhiteSpace(eventType)) query = query.Where(e => e.EventType == eventType);
            if (start.HasValue) query = query.Where(e => e.CreatedAt >= start.Value);
            if (end.HasValue) query = query.Where(e => e.CreatedAt <= end.Value);

            var total = await query.CountAsync();
            var items = await query
                .OrderByDescending(e => e.CreatedAt).ThenByDescending(e => e.Id)
                .Skip((page - 1) * pageSize).Take(pageSize)
                .Select(e => new ConfigEventItem
                {
                    Id = e.Id,
                    DollyId = e.DollyId,
                    DollyName = e.Dolly!.Name,
                    SerialNumber = e.Dolly!.SerialNumber,
                    EventType = e.EventType,
                    ConfigVersion = e.ConfigVersion,
                    Message = e.Message,
                    CreatedAt = e.CreatedAt
                })
                .ToListAsync();

            return new ConfigEventPageResponse { Total = total, Page = page, PageSize = pageSize, Items = items };
        }
    }
}
