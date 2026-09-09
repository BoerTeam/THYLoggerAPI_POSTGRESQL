using ClosedXML.Excel;
using Dashboard.DTO;
using Dashboard.Models;
using Dashboard.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Security.Claims;

namespace Dashboard.Controllers
{
    public class HomeController : Controller
    {
        private readonly IApiService _apiService;
        private readonly ILogger<HomeController> _logger;

        public HomeController(IApiService apiService, ILogger<HomeController> logger)
        {
            _apiService = apiService;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            // Tüm istekler asenkron olarak paralel çaðrýlabilir
            var nemTask = _apiService.GetAsync<List<Nem>>("api/Nem/Get");
            var sicaklikTask = _apiService.GetAsync<List<Sicaklik>>("api/Sicaklik");
            var dolubosTask = _apiService.GetAsync<List<BosDolu>>("api/DoluBos/Get");
            var dollyTask = _apiService.GetAsync<List<Dolly>>("api/Dolly/Get");
            var gpsTask = _apiService.GetAsync<List<Gpsdatum>>("api/Gps/Get");

            await Task.WhenAll(nemTask, sicaklikTask, dolubosTask, dollyTask, gpsTask);

            var multiModel = new MultiModel
            {
                nem = await nemTask ?? new List<Nem>(),
                Sicaklik = await sicaklikTask ?? new List<Sicaklik>(),
                bosDolu = await dolubosTask ?? new List<BosDolu>(),
                DollyList = await dollyTask ?? new List<Dolly>(),
                gpsdatum = await gpsTask ?? new List<Gpsdatum>()
            };

            return View(multiModel);
        }

        [HttpGet]
        public async Task<IActionResult> ExportToExcel(int? dollyId, DateTime startDate, DateTime endDate)
        {
            var startUtc = DateTime.SpecifyKind(startDate, DateTimeKind.Utc);
            var endUtc = DateTime.SpecifyKind(endDate, DateTimeKind.Utc).AddDays(1).AddTicks(-1);

            var tumSicakliklar = await _apiService.GetAsync<List<Sicaklik>>("api/Sicaklik") ?? new List<Sicaklik>();
            var tumNemler = await _apiService.GetAsync<List<Nem>>("api/Nem/Get") ?? new List<Nem>();
            var tumGpsler = await _apiService.GetAsync<List<Gpsdatum>>("api/Gps/Get") ?? new List<Gpsdatum>();
            var tumDollyler = await _apiService.GetAsync<List<Dolly>>("api/Dolly/Get") ?? new List<Dolly>();

            var dollyDict = tumDollyler.ToDictionary(x => x.Id, x => x.Name);

            var filteredSicaklik = tumSicakliklar
                .Where(x => (!dollyId.HasValue || x.DollyId == dollyId) && x.Time >= startUtc && x.Time <= endUtc)
                .OrderByDescending(x => x.Time)
                .ToList();

            var filteredNem = tumNemler
                .Where(x => (!dollyId.HasValue || x.DollyId == dollyId) && x.Time >= startUtc && x.Time <= endUtc)
                .OrderByDescending(x => x.Time)
                .ToList();

            var filteredGps = tumGpsler
                .Where(x => (!dollyId.HasValue || x.DollyId == dollyId) && x.Time >= startUtc && x.Time <= endUtc)
                .OrderByDescending(x => x.Time)
                .ToList();

            using (var workbook = new XLWorkbook())
            {
                var wsTemp = workbook.Worksheets.Add("Sýcaklýk Verileri");
                wsTemp.Cell(1, 1).Value = "Dolly Adý";
                wsTemp.Cell(1, 2).Value = "Tarih / Saat";
                wsTemp.Cell(1, 3).Value = "Sýcaklýk (°C)";

                int row = 2;
                foreach (var item in filteredSicaklik)
                {
                    wsTemp.Cell(row, 1).Value = dollyDict.TryGetValue(item.DollyId, out var name) ? name : item.DollyId.ToString();
                    wsTemp.Cell(row, 2).Value = item.Time?.ToString("dd.MM.yyyy HH:mm:ss");
                    wsTemp.Cell(row, 3).Value = item.Sicaklik1;
                    row++;
                }
                wsTemp.Columns().AdjustToContents();

                var wsHum = workbook.Worksheets.Add("Nem Verileri");
                wsHum.Cell(1, 1).Value = "Dolly Adý";
                wsHum.Cell(1, 2).Value = "Tarih / Saat";
                wsHum.Cell(1, 3).Value = "Nem (%)";

                row = 2;
                foreach (var item in filteredNem)
                {
                    wsHum.Cell(row, 1).Value = dollyDict.TryGetValue(item.DollyId, out var name) ? name : item.DollyId.ToString();
                    wsHum.Cell(row, 2).Value = item.Time?.ToString("dd.MM.yyyy HH:mm:ss");
                    wsHum.Cell(row, 3).Value = item.Nem1;
                    row++;
                }
                wsHum.Columns().AdjustToContents();

                var wsGps = workbook.Worksheets.Add("GPS Konum Verileri");
                wsGps.Cell(1, 1).Value = "Dolly Adý";
                wsGps.Cell(1, 2).Value = "Tarih / Saat";
                wsGps.Cell(1, 3).Value = "Enlem (Lat)";
                wsGps.Cell(1, 4).Value = "Boylam (Lng)";

                row = 2;
                foreach (var item in filteredGps)
                {
                    wsGps.Cell(row, 1).Value = dollyDict.TryGetValue(item.DollyId, out var name) ? name : item.DollyId.ToString();
                    wsGps.Cell(row, 2).Value = item.Time?.ToString("dd.MM.yyyy HH:mm:ss");
                    wsGps.Cell(row, 3).Value = item.Latitude;
                    wsGps.Cell(row, 4).Value = item.Longitude;
                    row++;
                }
                wsGps.Columns().AdjustToContents();

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();
                    string fileName = $"Dolly_Rapor_{startDate:yyyyMMdd}_{endDate:yyyyMMdd}.xlsx";
                    return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
                }
            }
        }

        [HttpGet]
        public async Task<JsonResult> GetLatestData(int id)
        {
            var sicakliklar = await _apiService.GetAsync<List<Sicaklik>>("api/Sicaklik") ?? new List<Sicaklik>();
            var nemler = await _apiService.GetAsync<List<Nem>>("api/Nem/Get") ?? new List<Nem>();
            var gpsler = await _apiService.GetAsync<List<Gpsdatum>>("api/Gps/Get") ?? new List<Gpsdatum>();
            var durumlar = await _apiService.GetAsync<List<BosDolu>>("api/DoluBos/Get") ?? new List<BosDolu>();

            var sonSicaklik = sicakliklar.Where(x => x.DollyId == id).OrderByDescending(x => x.Time).FirstOrDefault();
            var sonNem = nemler.Where(x => x.DollyId == id).OrderByDescending(x => x.Time).FirstOrDefault();
            var sonGps = gpsler.Where(x => x.DollyId == id).OrderByDescending(x => x.Time).FirstOrDefault();
            var sonDurum = durumlar.Where(x => x.DollyId == id).OrderByDescending(x => x.Time).FirstOrDefault();

            return Json(new
            {
                sicaklik = sonSicaklik?.Sicaklik1 ?? 0,
                nem = sonNem?.Nem1 ?? 0,
                lat = sonGps?.Latitude,
                lng = sonGps?.Longitude,
                isFull = sonDurum?.SensorDegeri ?? false
            });
        }

        [HttpGet]
        public async Task<IActionResult> GetHistoryData(int id, DateTime? start, DateTime? end)
        {
            var queryParams = new Dictionary<string, string> { { "id", id.ToString() } };
            if (start.HasValue) queryParams.Add("start", start.Value.ToString("yyyy-MM-ddTHH:mm:ss"));
            if (end.HasValue) queryParams.Add("end", end.Value.ToString("yyyy-MM-ddTHH:mm:ss"));

            var data = await _apiService.GetAsync<List<GPSHistoryModel>>("api/Gps/GetHistoryData", queryParams) ?? new List<GPSHistoryModel>();
            return Json(data);
        }

        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            // 1. API'ye login isteði atýlýr
            var response = await _apiService.PostAsync<LoginResponseDto, LoginViewModel>("api/auth/login", model);

            if (response != null && !string.IsNullOrEmpty(response.Token))
            {
                // 2. Claim'ler oluþturulur
                var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, response.Username),
            new Claim(ClaimTypes.Role, response.Role),
            new Claim("JWToken", response.Token) // API isteklerinde kullanýlacak JWT
        };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = model.RememberMe
                };

                // 3. Oturum Açýlýr (Cookie Yazýlýr)
                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity),
                    authProperties);

                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError("", "Kullanýcý adý veya þifre hatalý.");
            return View(model);
        }

        public class GPSHistoryModel
        {
            public double Lat { get; set; }
            public double Lng { get; set; }
            public string Time { get; set; }
        }

        public IActionResult Privacy() => View();

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}