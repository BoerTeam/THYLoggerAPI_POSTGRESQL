using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Text.Json;
using THYLoggerAPI_POSTGRESQL.Model;

namespace THYLoggerAPI_POSTGRESQL.Interceptors;

public class AuditInterceptor : SaveChangesInterceptor
{
    // 1. Senkron Kayıt Öncesi
    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        CreateAuditLogs(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    // 2. Asenkron Kayıt Öncesi
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        CreateAuditLogs(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void CreateAuditLogs(DbContext? dbContext)
    {
        if (dbContext == null) return;

        // Sonsuz döngüyü önlemek için ChangeTracker'ı donduruyoruz
        dbContext.ChangeTracker.DetectChanges();

        var auditEntries = new List<AuditLog>();

        // Sadece işlem gören gerçek varlıkları filtreliyoruz
        var entries = dbContext.ChangeTracker.Entries()
            .Where(e => e.Entity is not AuditLog &&
                       (e.State == EntityState.Added || e.State == EntityState.Modified || e.State == EntityState.Deleted))
            .ToList();

        foreach (var entry in entries)
        {
            var oldValues = new Dictionary<string, object?>();
            var newValues = new Dictionary<string, object?>();
            string primaryKey = string.Empty;

            foreach (var property in entry.Properties)
            {
                // Primary Key Alanı
                if (property.Metadata.IsPrimaryKey())
                {
                    primaryKey = property.CurrentValue?.ToString() ?? string.Empty;
                    continue;
                }

                string propertyName = property.Metadata.Name;

                switch (entry.State)
                {
                    case EntityState.Added:
                        newValues[propertyName] = property.CurrentValue;
                        break;

                    case EntityState.Deleted:
                        oldValues[propertyName] = property.OriginalValue;
                        break;

                    case EntityState.Modified:
                        if (property.IsModified)
                        {
                            oldValues[propertyName] = property.OriginalValue;
                            newValues[propertyName] = property.CurrentValue;
                        }
                        break;
                }
            }

            // Yeni eklenen nesnelerde ID henüz DB tarafından üretilmediyse geçici etiket koyuyoruz
            if (entry.State == EntityState.Added && (primaryKey == "0" || string.IsNullOrEmpty(primaryKey)))
            {
                primaryKey = "Auto-Generated";
            }

            auditEntries.Add(new AuditLog
            {
                UserId = null, // İleride IHttpContextAccessor ile JWT/Session'dan doldurulabilir
                EntityName = entry.Entity.GetType().Name,
                Action = entry.State.ToString(),
                PrimaryKey = primaryKey,
                OldValues = oldValues.Count == 0 ? null : JsonSerializer.Serialize(oldValues),
                NewValues = newValues.Count == 0 ? null : JsonSerializer.Serialize(newValues),
                CreatedAtUtc = DateTime.UtcNow
            });
        }

        if (auditEntries.Count > 0)
        {
            // Set<AuditLog>().AddRange yerine ChangeTracker'a takılmadan doğrudan ekliyoruz
            dbContext.Set<AuditLog>().AddRange(auditEntries);
        }
    }
}