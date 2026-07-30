namespace THYLoggerAPI_POSTGRESQL.Model;

public class AuditLog
{
    public long Id { get; set; }
    public string? UserId { get; set; } // Şimdilik null geçecek
    public string EntityName { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string PrimaryKey { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public string? OldValues { get; set; }
    public string? NewValues { get; set; }
}