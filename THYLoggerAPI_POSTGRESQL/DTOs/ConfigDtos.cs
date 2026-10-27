using System.Text.Json.Serialization;

namespace THYLoggerAPI_POSTGRESQL.DTOs
{
    public class ConfigGetRequest
    {
        public string? SerialNumber { get; set; }
        public string? DeviceType { get; set; }
        public string? AppVersion { get; set; }
        public int? ConfigVersion { get; set; }
    }

    public class ConfigVsControlRequest
    {
        public string? SerialNumber { get; set; }
        public int? ConfigVersion { get; set; }
        public DateTime? AppliedAt { get; set; }

        // İsteğe bağlı: cihaz uygulama sırasında hata aldıysa false + errorMessage gönderir (varsayılan: başarılı)
        public bool? Success { get; set; }
        public string? ErrorMessage { get; set; }
    }

    // Multipart: "LogFile" (dosya) ve "SerialNumber"
    public class DollyLogRequest
    {
        public IFormFile? LogFile { get; set; }
        public string? SerialNumber { get; set; }
    }

    // Dashboard -> API: seçili cihazlara (ve/veya cihaz modeline) yeni config yayınla
    public class ConfigPublishRequest
    {
        public List<int> DollyIds { get; set; } = new();
        public string? DeviceType { get; set; }
        public DeviceConfig? Config { get; set; }
        public string? Note { get; set; }
    }

    public class ConfigPublishResponse
    {
        public int UpdatedCount { get; set; }
        public List<ConfigPublishItem> Items { get; set; } = new();
    }

    public class ConfigPublishItem
    {
        public int DollyId { get; set; }
        public string SerialNumber { get; set; } = string.Empty;
        public int ConfigVersion { get; set; }
    }

    public class ConfigDeviceItem
    {
        public int DollyId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string SerialNumber { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public string? DeviceType { get; set; }
        public string? AppVersion { get; set; }
        public int? ServerConfigVersion { get; set; }
        public int? FetchedConfigVersion { get; set; }
        public DateTime? FetchedAt { get; set; }
        public int? AppliedConfigVersion { get; set; }
        public DateTime? AppliedAt { get; set; }
        public DateTime? LastSeenAt { get; set; }
        public string? LastError { get; set; }
        public DateTime? LastErrorAt { get; set; }

        // NoConfig | NeverSeen | Pending | Fetched | Applied | Error
        public string Status { get; set; } = string.Empty;
    }

    public class ConfigDeviceListResponse
    {
        public List<ConfigDeviceItem> Items { get; set; } = new();
        public List<string> DeviceTypes { get; set; } = new();
    }

    public class ConfigEventItem
    {
        public long Id { get; set; }
        public int DollyId { get; set; }
        public string DollyName { get; set; } = string.Empty;
        public string SerialNumber { get; set; } = string.Empty;
        public string EventType { get; set; } = string.Empty;
        public int? ConfigVersion { get; set; }
        public string? Message { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class ConfigEventPageResponse
    {
        public int Total { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public List<ConfigEventItem> Items { get; set; } = new();
    }

    public class ConfigGetResponse
    {
        public string Message { get; set; } = string.Empty;
        public int ConfigVersion { get; set; }
        public DeviceConfig? Config { get; set; }
    }

    public class DeviceConfig
    {
        public int ConfigPollIntervalSec { get; set; } = 300;
        public int ReadIntervalSec { get; set; } = 1;
        public int SendIntervalSec { get; set; } = 60;
        public int GpsIntervalSec { get; set; } = 10;
        public int GpsNoFixLogIntervalSec { get; set; } = 60;
        public DeviceSenders Senders { get; set; } = new();
        public List<AnalogChannel> AnalogChannels { get; set; } = new()
        {
            new AnalogChannel { Channel = "AI_A", Unit = "V", Send = false },
            new AnalogChannel { Channel = "AI_E", Unit = "mA", Send = true, Measurement = "nem" },
            new AnalogChannel { Channel = "AI_F", Unit = "mA", Send = true, Measurement = "sicaklik" }
        };
        public DeviceEndpoints Endpoints { get; set; } = new();
    }

    public class DeviceSenders
    {
        public bool Nem { get; set; } = true;
        public bool Sicaklik { get; set; } = true;
        public bool Gps { get; set; } = true;
        public bool DoluBos { get; set; } = true;
    }

    public class AnalogChannel
    {
        public string Channel { get; set; } = string.Empty;
        public string Unit { get; set; } = string.Empty;
        public bool Send { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Measurement { get; set; }
    }

    public class DeviceEndpoints
    {
        public string Nem { get; set; } = "/Nem/Add";
        public string Sicaklik { get; set; } = "/Sicaklik/Add";
        public string Gps { get; set; } = "/Gps/Add";
        public string DoluBos { get; set; } = "/DoluBos/Add";
    }
}
