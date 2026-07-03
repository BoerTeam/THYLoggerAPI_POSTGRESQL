using Dashboard.Models;
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
            if (Username == "admin" && Password == "1234")
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
