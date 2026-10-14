namespace Dashboard.DTO
{
    public class AuditLogItemDto
    {
        public long Id { get; set; }
        public string? UserId { get; set; }
        public string EntityName { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public string PrimaryKey { get; set; } = string.Empty;
        public DateTime CreatedAtUtc { get; set; }
        public string? OldValues { get; set; }
        public string? NewValues { get; set; }
    }

    public class AuditLogPageDto
    {
        public int Total { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public List<AuditLogItemDto> Items { get; set; } = new();
    }

    public class AuditLogFilterOptionsDto
    {
        public List<string> Entities { get; set; } = new();
        public List<string> Actions { get; set; } = new();
    }

    // Sayfa filtre formu (tarihler Turkiye saati)
    public class AuditLogFilterViewModel
    {
        public string? EntityName { get; set; }
        public string? Action { get; set; }
        public string? UserId { get; set; }
        public string? PrimaryKey { get; set; }
        public string? Search { get; set; }
        public DateTime? Start { get; set; }
        public DateTime? End { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 50;
    }

    public class AuditLogIndexViewModel
    {
        public AuditLogFilterViewModel Filter { get; set; } = new();
        public AuditLogPageDto Result { get; set; } = new();
        public AuditLogFilterOptionsDto Options { get; set; } = new();
        public bool ApiError { get; set; }
    }
}
