using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Text.Json;
using THYLoggerAPI_POSTGRESQL.Model;

namespace THYLoggerAPI_POSTGRESQL.Interceptors;

public class AuditInterceptor : SaveChangesInterceptor
{
    // Senkron _context.SaveChanges() çağrıları için:
    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        AuditEntities(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    // Asenkron _context.SaveChangesAsync() çağrıları için:
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        AuditEntities(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void AuditEntities(DbContext? dbContext)
    {
        if (dbContext == null) return;

        var auditEntries = new List<AuditLog>();

        // ChangeTracker.Entries() ile takip edilen verileri alıyoruz
        var entries = dbContext.ChangeTracker.Entries().ToList();

        foreach (var entry in entries)
        {
            // AuditLog tablosunun kendisini ve değişmeyen verileri izleme
            if (entry.Entity is AuditLog || entry.State == EntityState.Detached || entry.State == EntityState.Unchanged)
                continue;

            var oldValues = new Dictionary<string, object?>();
            var newValues = new Dictionary<string, object?>();
            string primaryKey = string.Empty;

            foreach (var property in entry.Properties)
            {
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

            auditEntries.Add(new AuditLog
            {
                UserId = null, // Şimdilik NULL
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
            dbContext.Set<AuditLog>().AddRange(auditEntries);
        }
    }
}