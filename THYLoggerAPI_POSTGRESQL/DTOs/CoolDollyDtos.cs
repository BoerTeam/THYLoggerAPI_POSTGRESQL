using THYLoggerAPI_POSTGRESQL.Model;

namespace THYLoggerAPI_POSTGRESQL.DTOs
{
    // ==========================================================
    // BPM -> Cool Dolly (servis mapping dokümanı, bölüm 4)
    //
    // NOT: Tüm alanlar bilerek "string?" tanımlandı. Projede Nullable=enable olduğu için
    // non-nullable string'ler [ApiController] tarafından otomatik "required" sayılır ve
    // eksik alanda BPM'in beklediği formatta değil, ProblemDetails formatında 400 dönerdi.
    // Doğrulamayı serviste yapıp CD-xxx kodlu cevap dönüyoruz.
    // ==========================================================
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

    public class TerminateCoolDollyRequestDto
    {
        public string? OperationType { get; set; }
        public string? RequestReferenceNo { get; set; }
        public string? FlightNumber { get; set; }
        public string? FlightDate { get; set; }
        public string? CoolDollyNumber { get; set; }
    }

    // Doküman bölüm 6: status / message / referenceNumber / timestamp
    public class CoolDollyResponse
    {
        public string Status { get; set; } = "SUCCESS";
        public string Message { get; set; } = string.Empty;
        public string? ReferenceNumber { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        public static CoolDollyResponse Success(string message, string referenceNumber) => new()
        {
            Status = "SUCCESS",
            Message = message,
            ReferenceNumber = referenceNumber
        };

        public static CoolDollyResponse Error(string code, string message) => new()
        {
            Status = "ERROR",
            Message = $"{code}: {message}"
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
