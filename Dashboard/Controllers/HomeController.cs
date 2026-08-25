using ClosedXML.Excel;
using Dashboard.DTO;
using Dashboard.Models;
using DocumentFormat.OpenXml.InkML;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;


namespace Dashboard.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            MultiModel multiModel = new MultiModel();
            multiModel.nem = Models.NemMethod.GetAllNemMethod();
            multiModel.Sicaklik = Models.SicaklikMethod.GetAllSicaklikMethod();
            multiModel.bosDolu = Models.DoluBosMethod.GetAllDoluBosMethod();
            multiModel.DollyList = Models.DollyMethod.GetAllDolly();
            multiModel.gpsdatum = Models.GpsDatumMethod.GetAllGpsDatumMethod();
            return View(multiModel);
        }
        [HttpGet]
        public IActionResult ExportToExcel(int? dollyId, DateTime startDate, DateTime endDate)
        {
            // 1. Tarih aral��� s�n�rlar�n� ayarlayal�m
            var startUtc = DateTime.SpecifyKind(startDate, DateTimeKind.Utc);
            var endUtc = DateTime.SpecifyKind(endDate, DateTimeKind.Utc).AddDays(1).AddTicks(-1);

            // 2. Verileri do�rudan sizin projenizdeki Method'lar �zerinden �ekiyoruz
            var tumSicakliklar = Models.SicaklikMethod.GetAllSicaklikMethod() ?? new List<Sicaklik>();
            var tumNemler = Models.NemMethod.GetAllNemMethod() ?? new List<Nem>();
            var tumGpsler = Models.GpsDatumMethod.GetAllGpsDatumMethod() ?? new List<Gpsdatum>();
            var tumDollyler = Models.DollyMethod.GetAllDolly() ?? new List<Dolly>();

            // Dolly Id - Name e�le�mesi i�in s�zl�k (Dictionary)
            var dollyDict = tumDollyler.ToDictionary(x => x.Id, x => x.Name);

            // 3. �ekilen listeleri verilen Filtrelere (DollyId ve Tarih) g�re s�z�yoruz
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

            // 4. Excel Dosyas� Olu�turma (ClosedXML)
            using (var workbook = new XLWorkbook())
            {
                // --- TAB 1: S�cakl�k ---
                var wsTemp = workbook.Worksheets.Add("S�cakl�k Verileri");
                wsTemp.Cell(1, 1).Value = "Dolly Ad�";
                wsTemp.Cell(1, 2).Value = "Tarih / Saat";
                wsTemp.Cell(1, 3).Value = "S�cakl�k (�C)";

                int row = 2;
                foreach (var item in filteredSicaklik)
                {
                    wsTemp.Cell(row, 1).Value = dollyDict.TryGetValue(item.DollyId, out var name) ? name : item.DollyId.ToString();
                    wsTemp.Cell(row, 2).Value = item.Time?.ToString("dd.MM.yyyy HH:mm:ss");
                    wsTemp.Cell(row, 3).Value = item.Sicaklik1;
                    row++;
                }
                wsTemp.Columns().AdjustToContents();

                // --- TAB 2: Nem ---
                var wsHum = workbook.Worksheets.Add("Nem Verileri");
                wsHum.Cell(1, 1).Value = "Dolly Ad�";
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

                // --- TAB 3: GPS Konum ---
                var wsGps = workbook.Worksheets.Add("GPS Konum Verileri");
                wsGps.Cell(1, 1).Value = "Dolly Ad�";
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

                // 5. Dosyay� indirilebilir formatta d�nd�r�yoruz
                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();
                    string fileName = $"Dolly_Rapor_{startDate:yyyyMMdd}_{endDate:yyyyMMdd}.xlsx";
                    return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
                }
            }
        }
        public IActionResult Privacy()
        {
            return View();
        }
        public IActionResult Deneme()
        {
            MultiModel multiModel = new MultiModel();
            multiModel.nem = Models.NemMethod.GetAllNemMethod();
            multiModel.Sicaklik = Models.SicaklikMethod.GetAllSicaklikMethod();
            multiModel.bosDolu = Models.DoluBosMethod.GetAllDoluBosMethod();
            multiModel.DollyList = Models.DollyMethod.GetAllDolly();
            multiModel.gpsdatum = Models.GpsDatumMethod.GetAllGpsDatumMethod();
            return View(multiModel);
        }
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpGet]
        public JsonResult GetLatestData(int id)
        {
            // Belirli bir Dolly ID'sine ait en son kay�tlar� �ekiyoruz
            var sonSicaklik = Models.SicaklikMethod.GetAllSicaklikMethod()
                                .Where(x => x.DollyId == id).OrderByDescending(x => x.Time).FirstOrDefault();

            var sonNem = Models.NemMethod.GetAllNemMethod()
                                .Where(x => x.DollyId == id).OrderByDescending(x => x.Time).FirstOrDefault();

            var sonGps = Models.GpsDatumMethod.GetAllGpsDatumMethod()
                                .Where(x => x.DollyId == id).OrderByDescending(x => x.Time).FirstOrDefault();

            var sonDurum = Models.DoluBosMethod.GetAllDoluBosMethod()
                                .Where(x => x.DollyId == id).OrderByDescending(x => x.Time).FirstOrDefault();

            return Json(new
            {
                sicaklik = sonSicaklik?.Sicaklik1 ?? 0,
                nem = sonNem?.Nem1 ?? 0,
                lat = sonGps?.Latitude,
                lng = sonGps?.Longitude,
                isFull = sonDurum?.SensorDegeri ?? false
            });
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Home");
            }

            ViewData["ReturnUrl"] = string.IsNullOrWhiteSpace(returnUrl) ? Url.Action("Index", "Home") : returnUrl;
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(string? returnUrl = null)
        {
            var redirectUrl = Url.Action("Index", "Home") ?? "/";
            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                redirectUrl = returnUrl;
            }

            return Challenge(
                new AuthenticationProperties { RedirectUri = redirectUrl },
                OpenIdConnectDefaults.AuthenticationScheme);
        }

        [HttpGet]
        public IActionResult Logout()
        {
            return SignOut(
                new AuthenticationProperties { RedirectUri = Url.Action("Login", "Home") },
                CookieAuthenticationDefaults.AuthenticationScheme,
                OpenIdConnectDefaults.AuthenticationScheme);
        }

        [HttpGet]
        public IActionResult GetHistoryData(int id, DateTime? start, DateTime? end)
        {
            // Static metodu �a��r�yoruz. 
            // Tarihleri API'nin anlayaca�� ISO format�na (yyyy-MM-ddTHH:mm:ss) �evirerek g�nderiyoruz.
            var data = Models.GpsDatumMethod.GetHistoryData(
                id,
                start?.ToString("yyyy-MM-ddTHH:mm:ss"),
                end?.ToString("yyyy-MM-ddTHH:mm:ss")
            );

            // API'den liste bo� gelse bile GetHistoryData metodun 'new List<GPSHistoryModel>()' d�nd��� i�in 
            // null hatas� almazs�n, bo� dizi [] d�ner.
            return Json(data);
        }
    }
}
