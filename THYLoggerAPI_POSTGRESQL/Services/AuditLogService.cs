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

            var query = _context.AuditLogs.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(entityName)) query = query.Where(x => x.EntityName == entityName);
            if (!string.IsNullOrWhiteSpace(action)) query = query.Where(x => x.Action == action);
            if (!string.IsNullOrWhiteSpace(userId)) query = query.Where(x => x.UserId == userId);
            if (!string.IsNullOrWhiteSpace(primaryKey)) query = query.Where(x => x.PrimaryKey == primaryKey);
            if (startUtc.HasValue) query = query.Where(x => x.CreatedAtUtc >= startUtc.Value);
            if (endUtc.HasValue) query = query.Where(x => x.CreatedAtUtc <= endUtc.Value);
            if (!string.IsNullOrWhiteSpace(search))
            {
                var pattern = $"%{search.Trim()}%";
                query = query.Where(x =>
                    (x.OldValues != null && EF.Functions.ILike(x.OldValues, pattern)) ||
                    (x.NewValues != null && EF.Functions.ILike(x.NewValues, pattern)));
            }

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
