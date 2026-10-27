namespace Dashboard.DTO
{
    // API: GET api/Config/Default | POST api/Config/Publish ile aynı yapı (cihaza giden config)
    public class DeviceConfig
    {
        public int ConfigPollIntervalSec { get; set; } = 300;
        public int ReadIntervalSec { get; set; } = 1;
        public int SendIntervalSec { get; set; } = 60;
        public int GpsIntervalSec { get; set; } = 10;
        public int GpsNoFixLogIntervalSec { get; set; } = 60;
        public DeviceSenders Senders { get; set; } = new();
        public List<AnalogChannel> AnalogChannels { get; set; } = new();
        public DeviceEndpoints Endpoints { get; set; } = new();
    }

    public class DeviceSenders
    {
        public bool Nem { get; set; }
        public bool Sicaklik { get; set; }
        public bool Gps { get; set; }
        public bool DoluBos { get; set; }
    }

    public class AnalogChannel
    {
        public string Channel { get; set; } = string.Empty;
        public string Unit { get; set; } = string.Empty;
        public bool Send { get; set; }
        public string? Measurement { get; set; }
    }

    public class DeviceEndpoints
    {
        public string Nem { get; set; } = string.Empty;
        public string Sicaklik { get; set; } = string.Empty;
        public string Gps { get; set; } = string.Empty;
        public string DoluBos { get; set; } = string.Empty;
    }

    public class ConfigPublishRequest
    {
        public List<int> DollyIds { get; set; } = new();
        public DeviceConfig? Config { get; set; }
        public string? Note { get; set; }
    }

    public class ConfigPublishResponse
    {
        public int UpdatedCount { get; set; }
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

    public class ConfigDeviceListDto
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
        public DateTime CreatedAt { get; set; }     // Türkiye saati
    }

    public class ConfigEventPageDto
    {
        public int Total { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public List<ConfigEventItem> Items { get; set; } = new();
    }

    // Gönderim ekranı
    public class ConfigIndexViewModel
    {
        public ConfigDeviceListDto Devices { get; set; } = new();
        public DeviceConfig Config { get; set; } = new();
        public string? Note { get; set; }
        public bool ApiError { get; set; }
    }

    // Gönderim formu (POST)
    public class ConfigPublishForm
    {
        public List<int> DollyIds { get; set; } = new();
        public DeviceConfig Config { get; set; } = new();
        public string? Note { get; set; }
    }

    // Rapor ekranı
    public class ConfigReportFilter
    {
        public int? DollyId { get; set; }
        public string? EventType { get; set; }
        public DateTime? Start { get; set; }
        public DateTime? End { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 50;
    }

    public class ConfigReportViewModel
    {
        public ConfigReportFilter Filter { get; set; } = new();
        public ConfigDeviceListDto Devices { get; set; } = new();
        public ConfigEventPageDto Events { get; set; } = new();
        public bool ApiError { get; set; }
    }
}
