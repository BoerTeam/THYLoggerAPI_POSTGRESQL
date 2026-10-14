using Microsoft.AspNetCore.Mvc;
using THYLoggerAPI_POSTGRESQL.Services;

namespace THYLoggerAPI_POSTGRESQL.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuditLogsController : ControllerBase
    {
        private readonly AuditLogService _auditLogService;

        public AuditLogsController(AuditLogService auditLogService)
        {
            _auditLogService = auditLogService;
        }

        // GET: api/AuditLogs?entityName=&action=&userId=&primaryKey=&search=&start=&end=&page=1&pageSize=50
        // start/end UTC olarak yorumlanir.
        [HttpGet]
        public async Task<IActionResult> Get(
            [FromQuery] string? entityName, [FromQuery] string? action, [FromQuery] string? userId,
            [FromQuery] string? primaryKey, [FromQuery] string? search,
            [FromQuery] DateTime? start, [FromQuery] DateTime? end,
            [FromQuery] int page = 1, [FromQuery] int pageSize = 50)
        {
            try
            {
                var startUtc = start.HasValue ? DateTime.SpecifyKind(start.Value, DateTimeKind.Utc) : (DateTime?)null;
                var endUtc = end.HasValue ? DateTime.SpecifyKind(end.Value, DateTimeKind.Utc) : (DateTime?)null;
                return Ok(await _auditLogService.GetAsync(entityName, action, userId, primaryKey, search, startUtc, endUtc, page, pageSize));
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Denetim kayitlari alinirken hata olustu: " + ex.Message);
            }
        }

        // GET: api/AuditLogs/filters
        [HttpGet("filters")]
        public async Task<IActionResult> GetFilters()
        {
            try
            {
                return Ok(await _auditLogService.GetFilterOptionsAsync());
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Filtre secenekleri alinirken hata olustu: " + ex.Message);
            }
        }
    }
}
