namespace Dashboard.DTO
{
    // API: GET api/DollyRequest/GetAll  ->  { total, page, pageSize, items }
    // Not: FlightDate, API'den "yyyy-MM-dd" metni olarak gelir (DateOnly Newtonsoft ile sorunsuz bağlanmadığı için string tutulur).
    public class DollyRequestItemDto
    {
        public int Id { get; set; }
        public string ReferenceNumber { get; set; } = string.Empty;

        public int DollyId { get; set; }
        public string CoolDollyName { get; set; } = string.Empty;
        public string CoolDollySerialNumber { get; set; } = string.Empty;

        public string AwbNo { get; set; } = string.Empty;
        public string OperationType { get; set; } = string.Empty;   // INBOUND / OUTBOUND
        public string FlightNo { get; set; } = string.Empty;
        public string FlightDate { get; set; } = string.Empty;      // yyyy-MM-dd
        public string DepartureStation { get; set; } = string.Empty;
        public string ArrivalStation { get; set; } = string.Empty;
        public string? SccCode { get; set; }
        public string UldNo { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;          // Open / Terminated
        public DateTime CreatedAt { get; set; }                     // UTC
        public DateTime? TerminatedAt { get; set; }                 // UTC
    }

    public class DollyRequestPageDto
    {
        public int Total { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public List<DollyRequestItemDto> Items { get; set; } = new();
    }

    // Sayfa filtre formu
    public class DollyRequestFilterViewModel
    {
        public string? CoolDollyNo { get; set; }
        public string? AwbNo { get; set; }
        public string? UldNo { get; set; }
        public string? FlightNo { get; set; }
        public string? FlightDateFrom { get; set; }   // yyyy-MM-dd
        public string? FlightDateTo { get; set; }     // yyyy-MM-dd
        public string? OperationType { get; set; }    // INBOUND / OUTBOUND / boş = tümü

        // Open = Dolly'nin içinde şu an olanlar (varsayılan), Terminated = sonlandırılmışlar, All = hepsi
        public string Status { get; set; } = "Open";

        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 50;
    }

    public class DollyRequestIndexViewModel
    {
        public DollyRequestFilterViewModel Filter { get; set; } = new();
        public DollyRequestPageDto Result { get; set; } = new();
        public bool ApiError { get; set; }
    }
}