using System.Text.Json.Serialization;
using THYLoggerAPI_POSTGRESQL.Model;

namespace THYLoggerAPI_POSTGRESQL.DTOs
{
    // ==========================================================
    // BPM -> Cool Dolly (Servis Mapping Dokümanı v1.1, Bölüm 4)
    // ==========================================================

    // Doküman Bölüm 4.1: Create Request (9 alan)
    public class CreateCoolDollyRequestDto
    {
        public string? OperationType { get; set; }      // INBOUND / OUTBOUND
        public string? AwbNumber { get; set; }
        public string? FlightNumber { get; set; }       // TK0001
        public string? FlightDate { get; set; }         // yyyy-MM-dd
        public string? DepartureStation { get; set; }   // 3 harf
        public string? ArrivalStation { get; set; }     // 3 harf
        public string? SccCode { get; set; }
        public string? UldNumber { get; set; }
        public string? CoolDollyNumber { get; set; }    // max 20
    }

    // Doküman Bölüm 4.3: Update Request (Create alanları + requestReferenceNo)
    public class UpdateCoolDollyRequestDto
    {
        public string? OperationType { get; set; }
        public string? AwbNumber { get; set; }
        public string? FlightNumber { get; set; }
        public string? FlightDate { get; set; }
        public string? DepartureStation { get; set; }
        public string? ArrivalStation { get; set; }
        public string? SccCode { get; set; }
        public string? UldNumber { get; set; }
        public string? CoolDollyNumber { get; set; }
        public string? RequestReferenceNo { get; set; }
    }

    // Doküman Bölüm 4.2: Terminate Request (6 alan)
    public class TerminateCoolDollyRequestDto
    {
        public string? OperationType { get; set; }
        public string? RequestReferenceNo { get; set; }
        public string? UldNumber { get; set; }
        public string? FlightNumber { get; set; }
        public string? FlightDate { get; set; }
        public string? CoolDollyNumber { get; set; }
    }

    // Doküman Bölüm 6.1 & 6.2: Standart Response
    public class CoolDollyResponse
    {
        public string Status { get; set; } = "SUCCESS";
        public string Message { get; set; } = string.Empty;
        public string? UldNumber { get; set; }
        public string? ReferenceNumber { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? ErrorCode { get; set; }

        public static CoolDollyResponse Success(string message, string? uldNumber, string referenceNumber) => new()
        {
            Status = "SUCCESS",
            Message = message,
            UldNumber = uldNumber,
            ReferenceNumber = referenceNumber,
            Timestamp = DateTime.UtcNow
        };

        public static CoolDollyResponse Error(string code, string message, string? uldNumber = null, string? referenceNumber = null) => new()
        {
            Status = "ERROR",
            Message = message,
            UldNumber = uldNumber,
            ReferenceNumber = referenceNumber,
            ErrorCode = code,
            Timestamp = DateTime.UtcNow
        };
    }

    // ==========================================================
    // Panel (liste ekranı) için
    // ==========================================================
    public class DollyRequestFilter
    {
        public string? CoolDollyNo { get; set; }     // Dolly adı veya seri no (içeren)
        public string? AwbNo { get; set; }
        public string? UldNo { get; set; }
        public string? FlightNo { get; set; }
        public DateOnly? FlightDateFrom { get; set; }
        public DateOnly? FlightDateTo { get; set; }
        public DollyOperationType? OperationType { get; set; }
        public DollyRequestStatus? Status { get; set; }   // boşsa hepsi; liste ekranı Open göndermeli

        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 50;
    }

    public class DollyRequestListItem
    {
        public int Id { get; set; }
        public string ReferenceNumber { get; set; } = string.Empty;

        public int DollyId { get; set; }
        public string CoolDollyName { get; set; } = string.Empty;
        public string CoolDollySerialNumber { get; set; } = string.Empty;

        public string AwbNo { get; set; } = string.Empty;
        public DollyOperationType OperationType { get; set; }
        public string FlightNo { get; set; } = string.Empty;
        public DateOnly FlightDate { get; set; }
        public string DepartureStation { get; set; } = string.Empty;
        public string ArrivalStation { get; set; } = string.Empty;
        public string? SccCode { get; set; }
        public string UldNo { get; set; } = string.Empty;

        public DollyRequestStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? TerminatedAt { get; set; }
    }

    public class PagedResult<T>
    {
        public List<T> Items { get; set; } = new();
        public int Total { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
    }
}