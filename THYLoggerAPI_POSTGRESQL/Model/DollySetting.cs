using System.ComponentModel.DataAnnotations;

namespace THYLoggerAPI_POSTGRESQL.Model
{
    /// <summary>
    /// Cihaza (Dolly) ait uzaktan konfigürasyon. Her Dolly için tek satır.
    /// ConfigVersion her yeni konfigürasyonda artar; cihaz kendi sürümünü gönderir, farklıysa yeni config alır.
    /// </summary>
    public class DollySetting
    {
        [Key]
        public int Id { get; set; }

        public int DollyId { get; set; }
        public Dolly? Dolly { get; set; }

        public int ConfigVersion { get; set; }

        // Cihaza gönderilen config gövdesi (jsonb)
        public string ConfigJson { get; set; } = "{}";

        // Cihazdan gelen bilgiler
        public string? DeviceType { get; set; }
        public string? AppVersion { get; set; }
        public int? AppliedConfigVersion { get; set; }
        public DateTime? AppliedAt { get; set; }
        public DateTime? LastSeenAt { get; set; }

        // Cihaz config'i ne zaman / hangi sürümü çekti (uygulamadan önceki adım)
        public int? FetchedConfigVersion { get; set; }
        public DateTime? FetchedAt { get; set; }

        // Cihazın bildirdiği son hata (uygulama başarısız olduysa)
        public string? LastError { get; set; }
        public DateTime? LastErrorAt { get; set; }

        public DateTime UpdatedAt { get; set; }
    }
}
