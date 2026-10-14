using Dashboard.DTO;
using Dashboard.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dashboard.Controllers
{
    [Authorize]
    public class AuditLogController : Controller
    {
        private static readonly TimeZoneInfo TurkeyTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Turkey Standard Time");

        private readonly IApiService _apiService;

        public AuditLogController(IApiService apiService)
        {
            _apiService = apiService;
        }

        // Denetim Kayitlari - yalnizca Admin
        [HttpGet]
        public async Task<IActionResult> Index(AuditLogFilterViewModel filter)
        {
            if (!User.IsInRole("Admin"))
            {
                return RedirectToAction("AccessDenied", "Home");
            }

            filter.Page = Math.Max(1, filter.Page);
            filter.PageSize = Math.Clamp(filter.PageSize, 10, 200);

            var query = new Dictionary<string, string>
            {
                { "page", filter.Page.ToString() },
                { "pageSize", filter.PageSize.ToString() }
            };
            void Add(string key, string? value) { if (!string.IsNullOrWhiteSpace(value)) query[key] = value.Trim(); }
            Add("entityName", filter.EntityName);
            Add("action", filter.Action);
            Add("userId", filter.UserId);
            Add("primaryKey", filter.PrimaryKey);
            Add("search", filter.Search);

            // Formdaki tarihler Turkiye saati; API UTC bekler
            if (filter.Start.HasValue)
                query["start"] = TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(filter.Start.Value, DateTimeKind.Unspecified), TurkeyTimeZone).ToString("yyyy-MM-ddTHH:mm:ss");
            if (filter.End.HasValue)
                query["end"] = TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(filter.End.Value, DateTimeKind.Unspecified), TurkeyTimeZone).ToString("yyyy-MM-ddTHH:mm:ss");

            var resultTask = _apiService.GetAsync<AuditLogPageDto>("api/AuditLogs", query);
            var optionsTask = _apiService.GetAsync<AuditLogFilterOptionsDto>("api/AuditLogs/filters");
            await Task.WhenAll(resultTask, optionsTask);

            var model = new AuditLogIndexViewModel
            {
                Filter = filter,
                Result = resultTask.Result ?? new AuditLogPageDto(),
                Options = optionsTask.Result ?? new AuditLogFilterOptionsDto(),
                ApiError = resultTask.Result == null
            };
            return View(model);
        }
    }
}
