using ClosedXML.Excel;
using Dashboard.DTO;
using Dashboard.Models;
using DocumentFormat.OpenXml.InkML;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;


namespace Dashboard.Controllers
{
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
            // 1. Tarih aralýðý sýnýrlarýný ayarlayalým
            var startUtc = DateTime.SpecifyKind(startDate, DateTimeKind.Utc);
            var endUtc = DateTime.SpecifyKind(endDate, DateTimeKind.Utc).AddDays(1).AddTicks(-1);

            // 2. Verileri doðrudan sizin projenizdeki Method'lar üzerinden çekiyoruz
            var tumSicakliklar = Models.SicaklikMethod.GetAllSicaklikMethod() ?? new List<Sicaklik>();
            var tumNemler = Models.NemMethod.GetAllNemMethod() ?? new List<Nem>();
            var tumGpsler = Models.GpsDatumMethod.GetAllGpsDatumMethod() ?? new List<Gpsdatum>();
            var tumDollyler = Models.DollyMethod.GetAllDolly() ?? new List<Dolly>();

            // Dolly Id - Name eþleþmesi için sözlük (Dictionary)
            var dollyDict = tumDollyler.ToDictionary(x => x.Id, x => x.Name);

            // 3. Çekilen listeleri verilen Filtrelere (DollyId ve Tarih) göre süzüyoruz
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

            // 4. Excel Dosyasý Oluþturma (ClosedXML)
            using (var workbook = new XLWorkbook())
            {
                // --- TAB 1: Sýcaklýk ---
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

                // --- TAB 2: Nem ---
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

                // --- TAB 3: GPS Konum ---
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

                // 5. Dosyayý indirilebilir formatta döndürüyoruz
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
            // Belirli bir Dolly ID'sine ait en son kayýtlarý çekiyoruz
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

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string Username, string Password)
        {
            // Burada API'den veya DB'den doðrulama yapabilirsin
            if (Username == "zkitapci" && Password == "7k#9P2x")
            {
                // Basit bir örnek: Cookie Authentication eklenebilir
                return RedirectToAction("Index", "Home");
            }

            ViewBag.Error = "Kullanýcý adý veya þifre hatalý!";
            return View();
        }

        [HttpGet]
        public IActionResult GetHistoryData(int id, DateTime? start, DateTime? end)
        {
            // Static metodu çaðýrýyoruz. 
            // Tarihleri API'nin anlayacaðý ISO formatýna (yyyy-MM-ddTHH:mm:ss) çevirerek gönderiyoruz.
            var data = Models.GpsDatumMethod.GetHistoryData(
                id,
                start?.ToString("yyyy-MM-ddTHH:mm:ss"),
                end?.ToString("yyyy-MM-ddTHH:mm:ss")
            );

            // API'den liste boþ gelse bile GetHistoryData metodun 'new List<GPSHistoryModel>()' döndüðü için 
            // null hatasý almazsýn, boþ dizi [] döner.
            return Json(data);
        }
    }
}
