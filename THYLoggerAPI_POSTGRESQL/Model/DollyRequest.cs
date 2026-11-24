using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace THYLoggerAPI_POSTGRESQL.Model
{
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

    public class DollyRequest
    {
        // Bir ULD'nin aynı anda yalnızca 1 açık talebi olabilir (KR-04 / CD-003 DB garantisi)
        public const string OpenPerUldIndexName = "UX_DollyRequests_OpenPerUld";

        [Key]
        public int Id { get; set; }

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
        public DateTime? UpdatedAt { get; set; }
    }
}