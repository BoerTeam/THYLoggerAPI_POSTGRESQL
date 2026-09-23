using ClosedXML.Excel;
using Dashboard.DTO;
using Dashboard.Models;
using Dashboard.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;

namespace Dashboard.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly IApiService _apiService;
        private readonly ILogger<HomeController> _logger;
        private readonly IConfiguration _config;

        public HomeController(IApiService apiService, ILogger<HomeController> logger, IConfiguration config)
        {
            _apiService = apiService;
            _logger = logger;
            _config = config;
        }

        [Authorize(Policy = "DollyView")]
        public async Task<IActionResult> Index()
        {
            var nemTask = _apiService.GetAsync<List<Nem>>("api/Nem/GetAll");
            var sicaklikTask = _apiService.GetAsync<List<Sicaklik>>("api/Sicaklik/GetAll");
            var dolubosTask = _apiService.GetAsync<List<BosDolu>>("api/DoluBos/GetAll");
            var dollyTask = _apiService.GetAsync<List<Dolly>>("api/Dolly/GetAll");
            var gpsTask = _apiService.GetAsync<List<Gpsdatum>>("api/Gps/GetAll");

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
        [Authorize]
        public async Task<IActionResult> ExportToExcel(int? dollyId, DateTime startDate, DateTime endDate)
        {
            var startUtc = DateTime.SpecifyKind(startDate, DateTimeKind.Utc);
            var endUtc = DateTime.SpecifyKind(endDate, DateTimeKind.Utc);

            var tumSicakliklar = await _apiService.GetAsync<List<Sicaklik>>("api/Sicaklik") ?? new List<Sicaklik>();
            var tumNemler = await _apiService.GetAsync<List<Nem>>("api/Nem/Get") ?? new List<Nem>();
            var tumGpsler = await _apiService.GetAsync<List<Gpsdatum>>("api/Gps/Get") ?? new List<Gpsdatum>();
            var tumDollyler = await _apiService.GetAsync<List<Dolly>>("api/Dolly/Get") ?? new List<Dolly>();

            var dollyDict = tumDollyler.ToDictionary(x => x.Id, x => x.Name);
            var trCulture = new System.Globalization.CultureInfo("tr-TR");

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
                // 1. Sýcaklýk Sayfasý
                var wsTemp = workbook.Worksheets.Add("Sýcaklýk Verileri");
                wsTemp.Cell(1, 1).Value = "Dolly Adý";
                wsTemp.Cell(1, 2).Value = "Tarih / Saat";
                wsTemp.Cell(1, 3).Value = "Sýcaklýk (°C)";

                int row = 2;
                foreach (var item in filteredSicaklik)
                {
                    wsTemp.Cell(row, 1).Value = dollyDict.TryGetValue(item.DollyId, out var name) ? name : item.DollyId.ToString();

                    if (item.Time.HasValue)
                    {
                        var cell = wsTemp.Cell(row, 2);
                        // String metin formatýnda 24 saatlik yazdýrma
                        cell.Value = item.Time.Value.ToString("dd.MM.yyyy HH:mm:ss", trCulture);
                        cell.Style.NumberFormat.Format = "@"; // Metin (Text) formatýna zorlama
                    }

                    wsTemp.Cell(row, 3).Value = item.Sicaklik1;
                    row++;
                }
                wsTemp.Columns().AdjustToContents();

                // 2. Nem Sayfasý
                var wsHum = workbook.Worksheets.Add("Nem Verileri");
                wsHum.Cell(1, 1).Value = "Dolly Adý";
                wsHum.Cell(1, 2).Value = "Tarih / Saat";
                wsHum.Cell(1, 3).Value = "Nem (%)";

                row = 2;
                foreach (var item in filteredNem)
                {
                    wsHum.Cell(row, 1).Value = dollyDict.TryGetValue(item.DollyId, out var name) ? name : item.DollyId.ToString();

                    if (item.Time.HasValue)
                    {
                        var cell = wsHum.Cell(row, 2);
                        cell.Value = item.Time.Value.ToString("dd.MM.yyyy HH:mm:ss", trCulture);
                        cell.Style.NumberFormat.Format = "@";
                    }

                    wsHum.Cell(row, 3).Value = item.Nem1;
                    row++;
                }
                wsHum.Columns().AdjustToContents();

                // 3. GPS Konum Sayfasý
                var wsGps = workbook.Worksheets.Add("GPS Konum Verileri");
                wsGps.Cell(1, 1).Value = "Dolly Adý";
                wsGps.Cell(1, 2).Value = "Tarih / Saat";
                wsGps.Cell(1, 3).Value = "Enlem (Lat)";
                wsGps.Cell(1, 4).Value = "Boylam (Lng)";

                row = 2;
                foreach (var item in filteredGps)
                {
                    wsGps.Cell(row, 1).Value = dollyDict.TryGetValue(item.DollyId, out var name) ? name : item.DollyId.ToString();

                    if (item.Time.HasValue)
                    {
                        var cell = wsGps.Cell(row, 2);
                        cell.Value = item.Time.Value.ToString("dd.MM.yyyy HH:mm:ss", trCulture);
                        cell.Style.NumberFormat.Format = "@";
                    }

                    wsGps.Cell(row, 3).Value = item.Latitude;
                    wsGps.Cell(row, 4).Value = item.Longitude;
                    row++;
                }
                wsGps.Columns().AdjustToContents();

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();

                    string fileName = $"Dolly_Rapor_{startDate.ToString("yyyyMMdd_HHmm", trCulture)}_{endDate.ToString("yyyyMMdd_HHmm", trCulture)}.xlsx";

                    return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
                }
            }
        }
        [HttpGet]
        [Authorize(Policy = "DollyView")]
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
        [Authorize(Policy = "DollyView")]
        public async Task<IActionResult> GetHistoryData(int id, DateTime? start, DateTime? end)
        {
            var queryParams = new Dictionary<string, string> { { "id", id.ToString() } };
            if (start.HasValue) queryParams.Add("start", start.Value.ToString("yyyy-MM-ddTHH:mm:ss"));
            if (end.HasValue) queryParams.Add("end", end.Value.ToString("yyyy-MM-ddTHH:mm:ss"));

            var data = await _apiService.GetAsync<List<GPSHistoryModel>>("api/Gps/GetHistoryData", queryParams) ?? new List<GPSHistoryModel>();
            return Json(data);
        }

        // ==========================================
        // OIDC / THY SSO ENTEGRASYON ALANI
        // ==========================================

        // 1. Kullanýcýyý THY SSO Giriþ Ekranýna Yönlendirir
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }


        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var response = await _apiService.PostAsync<LoginResponseDto, LoginViewModel>("api/auth/login", model);

            if (response != null && response.IsSuccess)
            {
                var claims = new List<Claim>
         {
             new Claim(ClaimTypes.NameIdentifier, response.UserId.ToString()),
             new Claim(ClaimTypes.Name, response.UserName)
         };

                if (!string.IsNullOrEmpty(response.Token))
                {
                    claims.Add(new Claim("JWToken", response.Token));
                }

                if (response.Roles != null)
                {
                    foreach (var role in response.Roles)
                    {
                        claims.Add(new Claim(ClaimTypes.Role, role));
                    }
                }

                if (response.Permissions != null)
                {
                    foreach (var perm in response.Permissions)
                    {
                        claims.Add(new Claim("Permission", perm));
                    }
                }

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = model.RememberMe
                };

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity),
                    authProperties);

                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError("", response?.Message ?? "Kullanýcý adý veya þifre hatalý.");
            return View(model);
        }

        //[HttpGet]
        //[AllowAnonymous]
        //public IActionResult Login()
        //{
        //    if (User.Identity?.IsAuthenticated == true)
        //    {
        //        return RedirectToAction("Index", "Home");
        //    }

        //    // CSRF ve Replay korumasý için URL-Safe Base64 üretimi (Doküman Madde 3)
        //    byte[] stateBytes = RandomNumberGenerator.GetBytes(32);
        //    byte[] nonceBytes = RandomNumberGenerator.GetBytes(16);

        //    string state = Convert.ToBase64String(stateBytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');
        //    string nonce = Convert.ToBase64String(nonceBytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');

        //    var clientId = _config["OidcSettings:ClientId"] ?? "";
        //    var redirectUri = _config["OidcSettings:RedirectUri"] ?? "";
        //    var authorizeUrl = _config["OidcSettings:AuthorizeUrl"] ?? "";

        //    // scope appsettings.json dosyasýndan okunuyor (varsayýlan: openid profile email)
        //    var scope = _config["OidcSettings:Scope"] ?? "openid profile email";

        //    // URL Parametrelerinin güvenli þekilde oluþturulmasý
        //    string authRedirectUrl = $"{authorizeUrl}?response_type=code" +
        //                             $"&client_id={Uri.EscapeDataString(clientId)}" +
        //                             $"&scope={Uri.EscapeDataString(scope)}" +
        //                             $"&redirect_uri={Uri.EscapeDataString(redirectUri)}" +
        //                             $"&state={Uri.EscapeDataString(state)}" +
        //                             $"&nonce={Uri.EscapeDataString(nonce)}";

        //    return Redirect(authRedirectUrl);
        //}

        // 2. THY Portalýndan Doðrulama Sonrasý Dönülen Callback Adresi
        [HttpGet]
        [Route("Callback")]
        [Route("Home/Callback")]
        [AllowAnonymous]
        public async Task<IActionResult> Callback(string code, string state)
        {
            if (string.IsNullOrEmpty(code))
            {
                return RedirectToAction("AccessDenied");
            }

            // 1. Code -> Token Takasý
            var tokenRequest = new
            {
                grant_type = "authorization_code",
                code = code,
                client_id = _config["OidcSettings:ClientId"],
                redirect_uri = _config["OidcSettings:RedirectUri"]
            };

            var tokenResponse = await _apiService.PostAsync<OidcTokenResponseDto, object>(
                _config["OidcSettings:TokenUrl"] ?? "", tokenRequest);

            if (tokenResponse == null || string.IsNullOrEmpty(tokenResponse.IdToken))
            {
                return RedirectToAction("AccessDenied");
            }

            // 2. ID Token Decode Etme
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(tokenResponse.IdToken);

            string username = jwtToken.Claims.FirstOrDefault(c =>
                c.Type == "preferred_username" ||
                c.Type == "unique_name" ||
                c.Type == "sub")?.Value ?? "";

            string email = jwtToken.Claims.FirstOrDefault(c => c.Type == "email")?.Value ?? "";

            // 3. API'ye Ýstek At: SsoUserResponseDto Tipinde Kullanýcýyý Çek veya Oluþtur (JIT)
            var ssoUserDto = new { Username = username, Email = email };
            var userDetail = await _apiService.PostAsync<SsoUserResponseDto, object>("api/Users/get-or-create-sso-user", ssoUserDto);

            // 4. Claims Hazýrlýðý ve Oturum Açma
            var claims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, userDetail?.UserId.ToString() ?? username),
        new Claim(ClaimTypes.Name, userDetail?.UserName ?? username),
        new Claim("JWToken", tokenResponse.AccessToken ?? "")
    };

            // Roller Ekleniyor (String Listesi Olarak)
            if (userDetail?.Roles != null)
            {
                foreach (var roleName in userDetail.Roles)
                {
                    claims.Add(new Claim(ClaimTypes.Role, roleName));
                }
            }

            // Ýzin Kodlarý (DOLLY_VIEW, DOLLY_EDIT vb.) Ekleniyor
            if (userDetail?.Permissions != null)
            {
                foreach (var perm in userDetail.Permissions)
                {
                    claims.Add(new Claim("Permission", perm));
                }
            }

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity));

            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Home");
        }

        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
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

    public class OidcTokenResponseDto
    {
        public string? AccessToken { get; set; }
        public string? RefreshToken { get; set; }
        public string? IdToken { get; set; }
        public string? TokenType { get; set; }
        public int ExpiresIn { get; set; }
    }
    public class SsoUserResponseDto
    {
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public List<string> Roles { get; set; } = new();
        public List<string> Permissions { get; set; } = new();
    }
}