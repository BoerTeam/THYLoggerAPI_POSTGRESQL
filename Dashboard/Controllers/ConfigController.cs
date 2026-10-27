using Dashboard.DTO;
using Dashboard.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dashboard.Controllers
{
    [Authorize]
    public class ConfigController : Controller
    {
        private readonly IApiService _apiService;

        public ConfigController(IApiService apiService)
        {
            _apiService = apiService;
        }

        // Cihaz konfigürasyonu gönderme ekranı - yalnızca Admin
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            if (!User.IsInRole("Admin")) return RedirectToAction("AccessDenied", "Home");

            var devicesTask = _apiService.GetAsync<ConfigDeviceListDto>("api/Config/Devices");
            var defaultTask = _apiService.GetAsync<DeviceConfig>("api/Config/Default");
            await Task.WhenAll(devicesTask, defaultTask);

            return View(new ConfigIndexViewModel
            {
                Devices = devicesTask.Result ?? new ConfigDeviceListDto(),
                Config = defaultTask.Result ?? new DeviceConfig(),
                ApiError = devicesTask.Result == null || defaultTask.Result == null
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Publish(ConfigPublishForm form)
        {
            if (!User.IsInRole("Admin")) return RedirectToAction("AccessDenied", "Home");

            if (form.DollyIds == null || form.DollyIds.Count == 0)
            {
                TempData["ConfigError"] = "En az bir cihaz seçmelisiniz.";
                return RedirectToAction(nameof(Index));
            }

            var result = await _apiService.PostAsync<ConfigPublishResponse, ConfigPublishRequest>(
                "api/Config/Publish",
                new ConfigPublishRequest { DollyIds = form.DollyIds, Config = form.Config, Note = form.Note });

            if (result == null)
                TempData["ConfigError"] = "Konfigürasyon yayınlanamadı. Değerleri kontrol edin (süreler en az 1, endpointler '/' ile başlamalı, gönderilen kanallar için measurement dolu olmalı).";
            else
                TempData["ConfigSuccess"] = $"Konfigürasyon {result.UpdatedCount} cihaz için yayınlandı. Cihazlar bir sonraki kontrolde çekip uygulayacak.";

            return RedirectToAction(nameof(Index));
        }

        // Cihaz durumu + olay raporu - yalnızca Admin
        [HttpGet]
        public async Task<IActionResult> Report(ConfigReportFilter filter)
        {
            if (!User.IsInRole("Admin")) return RedirectToAction("AccessDenied", "Home");

            filter.Page = Math.Max(1, filter.Page);
            filter.PageSize = Math.Clamp(filter.PageSize, 10, 200);

            var query = new Dictionary<string, string>
            {
                { "page", filter.Page.ToString() },
                { "pageSize", filter.PageSize.ToString() }
            };
            if (filter.DollyId.HasValue) query["dollyId"] = filter.DollyId.Value.ToString();
            if (!string.IsNullOrWhiteSpace(filter.EventType)) query["eventType"] = filter.EventType;
            // Olay zamanları Türkiye saatinde saklandığı için dönüşüm yapılmaz
            if (filter.Start.HasValue) query["start"] = filter.Start.Value.ToString("yyyy-MM-ddTHH:mm:ss");
            if (filter.End.HasValue) query["end"] = filter.End.Value.ToString("yyyy-MM-ddTHH:mm:ss");

            var devicesTask = _apiService.GetAsync<ConfigDeviceListDto>("api/Config/Devices");
            var eventsTask = _apiService.GetAsync<ConfigEventPageDto>("api/Config/Events", query);
            await Task.WhenAll(devicesTask, eventsTask);

            return View(new ConfigReportViewModel
            {
                Filter = filter,
                Devices = devicesTask.Result ?? new ConfigDeviceListDto(),
                Events = eventsTask.Result ?? new ConfigEventPageDto(),
                ApiError = devicesTask.Result == null || eventsTask.Result == null
            });
        }
    }
}
