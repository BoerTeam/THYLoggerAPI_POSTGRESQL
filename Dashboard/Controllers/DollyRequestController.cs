using System.Globalization;
using Dashboard.DTO;
using Dashboard.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dashboard.Controllers
{
    [Authorize]
    public class DollyRequestController : Controller
    {
        private readonly IApiService _apiService;

        public DollyRequestController(IApiService apiService)
        {
            _apiService = apiService;
        }

        // Dolly Talepleri (Dolly içindeki ULD / AWB detayları) - Yalnızca DOLLY_VIEW veya Admin
        [HttpGet]
        public async Task<IActionResult> Index(DollyRequestFilterViewModel filter)
        {
            if (!User.HasClaim("Permission", "DOLLY_VIEW") && !User.IsInRole("Admin"))
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
            Add("coolDollyNo", filter.CoolDollyNo);
            Add("awbNo", filter.AwbNo);
            Add("uldNo", filter.UldNo);
            Add("flightNo", filter.FlightNo);

            // Geçersiz değerler API'de 400 üretmesin diye burada elenir
            if (filter.OperationType is "INBOUND" or "OUTBOUND")
                query["operationType"] = filter.OperationType;

            if (filter.Status is "Open" or "Terminated")
                query["status"] = filter.Status;
            else
                filter.Status = "All";

            if (IsDate(filter.FlightDateFrom)) query["flightDateFrom"] = filter.FlightDateFrom!;
            if (IsDate(filter.FlightDateTo)) query["flightDateTo"] = filter.FlightDateTo!;

            var result = await _apiService.GetAsync<DollyRequestPageDto>("api/DollyRequest/GetAll", query);

            var model = new DollyRequestIndexViewModel
            {
                Filter = filter,
                Result = result ?? new DollyRequestPageDto(),
                ApiError = result == null
            };
            return View(model);
        }

        private static bool IsDate(string? value) =>
            DateOnly.TryParseExact(value, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out _);
    }
}