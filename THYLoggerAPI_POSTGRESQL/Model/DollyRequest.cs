using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace THYLoggerAPI_POSTGRESQL.Model
{
    // Enum üzerindeki JsonConverter sayesinde hem API çıktısında hem AuditLog JSON'unda
    // sayı yerine "INBOUND" / "Open" gibi isimler görünür.
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum DollyOperationType
    {
        INBOUND = 1,
        OUTBOUND = 2
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum DollyRequestStatus
    {
        Open = 1,
        Terminated = 2
    }

    /// <summary>
    /// BPM'den gelen talep: bir ULD/AWB'nin bir Cool Dolly'ye atanması.
    /// Dolly ile ilişki DollyId üzerinden kurulur ("Cool Dolly No" -> Dolly.Name / Dolly.SerialNumber).
    /// </summary>
    public class DollyRequest
    {
        // Bir Dolly'nin aynı anda yalnızca 1 açık talebi olabilir (CD-002'nin DB seviyesinde garantisi)
        public const string OpenPerDollyIndexName = "UX_DollyRequests_OpenPerDolly";

        [Key]
        public int Id { get; set; }

        // Cool Dolly'nin ürettiği referans no (BPM terminate'te bunu geri gönderir)
        public string ReferenceNumber { get; set; } = string.Empty;

        public DollyOperationType OperationType { get; set; }

        public string AwbNumber { get; set; } = string.Empty;
        public string FlightNumber { get; set; } = string.Empty;
        public DateOnly FlightDate { get; set; }
        public string DepartureStation { get; set; } = string.Empty;
        public string ArrivalStation { get; set; } = string.Empty;
        public string? SccCode { get; set; }
        public string UldNumber { get; set; } = string.Empty;

        public int DollyId { get; set; }
        public Dolly? Dolly { get; set; }

        public DollyRequestStatus Status { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? TerminatedAt { get; set; }
    }
}
