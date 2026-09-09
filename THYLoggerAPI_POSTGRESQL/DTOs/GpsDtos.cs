namespace THYLoggerAPI_POSTGRESQL.DTOs
{
    public class GpsCreateResultDto
    {
        public bool IsSuccess { get; set; }
        public bool IsNotFound { get; set; }
        public string? ErrorMessage { get; set; }
        public string? Message { get; set; }
        public string? DeviceName { get; set; }
        public string? Location { get; set; }
    }

    public class GpsHistoryItemDto
    {
        public double? Lat { get; set; }
        public double? Lng { get; set; }
        public string Time { get; set; } = string.Empty;
    }
}