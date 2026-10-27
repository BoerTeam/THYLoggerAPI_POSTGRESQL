using System.ComponentModel.DataAnnotations;

namespace THYLoggerAPI_POSTGRESQL.Model
{
    public static class DollyConfigEventTypes
    {
        public const string Published = "Published";   // Dashboard'dan yeni config yayınlandı
        public const string Fetched = "Fetched";       // Cihaz yeni config'i çekti
        public const string Applied = "Applied";       // Cihaz config'i uyguladığını bildirdi
        public const string Error = "Error";           // Cihaz uygulama hatası bildirdi
        public const string LogUploaded = "LogUploaded";
    }

    /// <summary>Cihaz konfigürasyon yaşam döngüsü kaydı (rapor ekranı için).</summary>
    public class DollyConfigEvent
    {
        [Key]
        public long Id { get; set; }

        public int DollyId { get; set; }
        public Dolly? Dolly { get; set; }

        public string EventType { get; set; } = string.Empty;
        public int? ConfigVersion { get; set; }
        public string? Message { get; set; }

        // Türkiye saati (diğer cihaz zaman damgalarıyla aynı)
        public DateTime CreatedAt { get; set; }
    }
}
