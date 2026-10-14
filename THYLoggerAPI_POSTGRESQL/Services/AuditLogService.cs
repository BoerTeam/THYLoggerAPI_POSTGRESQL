using Microsoft.EntityFrameworkCore;
using THYLoggerAPI_POSTGRESQL.Context;

namespace THYLoggerAPI_POSTGRESQL.Services
{
    public class AuditLogService
    {
        private const int MaxPageSize = 200;

        private readonly ApplicationDbContext _context;

        public AuditLogService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<object> GetAsync(
            string? entityName, string? action, string? userId, string? primaryKey, string? search,
            DateTime? startUtc, DateTime? endUtc, int page, int pageSize)
        {
            page = Math.Max(1, page);
            pageSize = Math.Clamp(pageSize, 1, MaxPageSize);

            IQueryable<THYLoggerAPI_POSTGRESQL.Model.AuditLog> query;
            if (!string.IsNullOrWhiteSpace(search))
            {
                // OldValues/NewValues jsonb: ILIKE icin text'e cevrilmeli; LIKE joker karakterleri kacirilir
                var pattern = "%" + search.Trim().Replace("\\", "\\\\").Replace("%", "\\%").Replace("_", "\\_") + "%";
                query = _context.AuditLogs
                    .FromSqlInterpolated($"SELECT * FROM \"AuditLogs\" WHERE \"OldValues\"::text ILIKE {pattern} OR \"NewValues\"::text ILIKE {pattern}")
                    .AsNoTracking();
            }
            else
            {
                query = _context.AuditLogs.AsNoTracking();
            }

            if (!string.IsNullOrWhiteSpace(entityName)) query = query.Where(x => x.EntityName == entityName);
            if (!string.IsNullOrWhiteSpace(action)) query = query.Where(x => x.Action == action);
            if (!string.IsNullOrWhiteSpace(userId)) query = query.Where(x => x.UserId == userId);
            if (!string.IsNullOrWhiteSpace(primaryKey)) query = query.Where(x => x.PrimaryKey == primaryKey);
            if (startUtc.HasValue) query = query.Where(x => x.CreatedAtUtc >= startUtc.Value);
            if (endUtc.HasValue) query = query.Where(x => x.CreatedAtUtc <= endUtc.Value);

            var total = await query.CountAsync();
            var items = await query
                .OrderByDescending(x => x.CreatedAtUtc)
                .ThenByDescending(x => x.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new { total, page, pageSize, items };
        }

        // Filtre acilir listeleri icin mevcut varlik ve islem degerleri
        public async Task<object> GetFilterOptionsAsync()
        {
            var entities = await _context.AuditLogs.AsNoTracking().Select(x => x.EntityName).Distinct().OrderBy(x => x).ToListAsync();
            var actions = await _context.AuditLogs.AsNoTracking().Select(x => x.Action).Distinct().OrderBy(x => x).ToListAsync();
            return new { entities, actions };
        }
    }
}
