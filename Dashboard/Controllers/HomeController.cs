using ClosedXML.Excel;
using Dashboard.DTO;
using Dashboard.Models;
using Dashboard.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
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

        // Ana Sayfa (Dashboard) - Yalnýzca DASHBOARD_VIEW veya Admin
        public async Task<IActionResult> Index()
        {
            if (!User.HasClaim("Permission", "DASHBOARD_VIEW") && !User.IsInRole("Admin"))
            {
                return RedirectToAction("AccessDenied", "Home");
            }

            // Sadece her dolly'nin son kayitlari cekilir (tum tablolar degil)
            var snapshotTask = _apiService.GetAsync<LatestSnapshot>("api/Latest/GetAll");
            var dollyTask = _apiService.GetAsync<List<Dolly>>("api/Dolly/GetAll");
            var openTask = _apiService.GetAsync<List<DollyRequestItemDto>>("api/DollyRequest/GetOpen");

            await Task.WhenAll(snapshotTask, dollyTask, openTask);

            var snapshot = await snapshotTask ?? new LatestSnapshot();
            var multiModel = new MultiModel
            {
                nem = snapshot.Nem,
                Sicaklik = snapshot.Sicaklik,
                bosDolu = snapshot.BosDolu,
                DollyList = await dollyTask ?? new List<Dolly>(),
                gpsdatum = snapshot.Gps,
                OpenRequests = await openTask ?? new List<DollyRequestItemDto>()
            };

            return View(multiModel);
        }

        [HttpGet]
        public async Task<IActionResult> ExportToExcel(int? dollyId, DateTime startDate, DateTime endDate)
        {
            if (!User.HasClaim("Permission", "DOLLY_VIEW") && !User.IsInRole("Admin"))
            {
                return RedirectToAction("AccessDenied", "Home");
            }

            DateTime filterStart = startDate;
            DateTime filterEnd = endDate;

            var rangeParams = new Dictionary<string, string>
    {
        { "start", filterStart.ToString("yyyy-MM-ddTHH:mm:ss") },
        { "end", filterEnd.ToString("yyyy-MM-ddTHH:mm:ss") }
    };
            if (dollyId.HasValue) rangeParams.Add("dollyId", dollyId.Value.ToString());

            var tumSicakliklar = await _apiService.GetAsync<List<Sicaklik>>("api/Sicaklik/GetAll", rangeParams) ?? new List<Sicaklik>();
            var tumNemler = await _apiService.GetAsync<List<Nem>>("api/Nem/GetAll", rangeParams) ?? new List<Nem>();
            var tumGpsler = await _apiService.GetAsync<List<Gpsdatum>>("api/Gps/GetAll", rangeParams) ?? new List<Gpsdatum>();
            var tumDurumlar = await _apiService.GetAsync<List<BosDolu>>("api/DoluBos/GetAll", rangeParams) ?? new List<BosDolu>();
            var tumDollyler = await _apiService.GetAsync<List<Dolly>>("api/Dolly/GetAll") ?? new List<Dolly>();

            var dollyDict = tumDollyler.ToDictionary(x => x.Id, x => x.Name);
            var trCulture = new System.Globalization.CultureInfo("tr-TR");

            // Tüm sensör kayýtlarýný ortak bir listede birleþtiriyoruz
            var unifiedTelemetryList = new List<UnifiedTelemetryRow>();

            // 1. Sýcaklýk Verileri
            unifiedTelemetryList.AddRange(tumSicakliklar
                .Where(x => (!dollyId.HasValue || x.DollyId == dollyId) && x.Time.HasValue && x.Time.Value >= filterStart && x.Time.Value <= filterEnd)
                .Select(x => new UnifiedTelemetryRow
                {
                    DollyId = x.DollyId,
                    Time = x.Time!.Value,
                    SensorType = "Sýcaklýk",
                    Value = x.Sicaklik1.HasValue ? $"{x.Sicaklik1.Value:0.0} °C" : "-",
                    Details = "-"
                }));

            // 2. Nem Verileri
            unifiedTelemetryList.AddRange(tumNemler
                .Where(x => (!dollyId.HasValue || x.DollyId == dollyId) && x.Time.HasValue && x.Time.Value >= filterStart && x.Time.Value <= filterEnd)
                .Select(x => new UnifiedTelemetryRow
                {
                    DollyId = x.DollyId,
                    Time = x.Time!.Value,
                    SensorType = "Nem",
                    Value = x.Nem1.HasValue ? $"%{x.Nem1.Value:0}" : "-",
                    Details = "-"
                }));

            // 3. GPS Verileri
            unifiedTelemetryList.AddRange(tumGpsler
                .Where(x => (!dollyId.HasValue || x.DollyId == dollyId) && x.Time.HasValue && x.Time.Value >= filterStart && x.Time.Value <= filterEnd)
                .Select(x => new UnifiedTelemetryRow
                {
                    DollyId = x.DollyId,
                    Time = x.Time!.Value,
                    SensorType = "GPS Konum",
                    Value = $"{x.Latitude}, {x.Longitude}",
                    Details = $"Enlem: {x.Latitude} | Boylam: {x.Longitude}"
                }));

            // 4. Boþ/Dolu Durum Verileri
            unifiedTelemetryList.AddRange(tumDurumlar
                .Where(x => (!dollyId.HasValue || x.DollyId == dollyId) && x.Time.HasValue && x.Time.Value >= filterStart && x.Time.Value <= filterEnd)
                .Select(x => new UnifiedTelemetryRow
                {
                    DollyId = x.DollyId,
                    Time = x.Time!.Value,
                    SensorType = "Boþ / Dolu",
                    Value = x.SensorDegeri == true ? "Dolu" : "Boþ",
                    Details = "-"
                }));

            // Zamana göre yeniden eskiye kronolojik sýralama
            var sortedTelemetry = unifiedTelemetryList.OrderByDescending(x => x.Time).ToList();

            using (var workbook = new XLWorkbook())
            {
                var ws = workbook.Worksheets.Add("Tüm Telemetri Verileri");

                // Baþlýklar
                string[] headers = { "Dolly Adý", "Tarih / Saat", "Sensör / Veri Tipi", "Ölçülen Deðer", "Detay / Koordinat" };
                for (int i = 0; i < headers.Length; i++)
                {
                    var cell = ws.Cell(1, i + 1);
                    cell.Value = headers[i];
                    cell.Style.Font.Bold = true;
                    cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#7367F0"); // Kurumsal Mor/Mavi
                    cell.Style.Font.FontColor = XLColor.White;
                    cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                }

                int row = 2;
                foreach (var item in sortedTelemetry)
                {
                    ws.Cell(row, 1).Value = dollyDict.TryGetValue(item.DollyId, out var name) ? name : item.DollyId.ToString();

                    var dateCell = ws.Cell(row, 2);
                    dateCell.Value = item.Time.ToString("dd.MM.yyyy HH:mm:ss", trCulture);
                    dateCell.Style.NumberFormat.Format = "@";

                    ws.Cell(row, 3).Value = item.SensorType;
                    ws.Cell(row, 4).Value = item.Value;
                    ws.Cell(row, 5).Value = item.Details;

                    row++;
                }

                ws.Columns().AdjustToContents();

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();
                    string fileName = $"Dolly_Telemetri_Raporu_{startDate.ToString("yyyyMMdd_HHmm", trCulture)}_{endDate.ToString("yyyyMMdd_HHmm", trCulture)}.xlsx";
                    return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
                }
            }
        }

        // Ortak liste için yardýmcý model
        public class UnifiedTelemetryRow
        {
            public int DollyId { get; set; }
            public DateTime Time { get; set; }
            public string SensorType { get; set; } = string.Empty;
            public string Value { get; set; } = string.Empty;
            public string Details { get; set; } = string.Empty;
        }

        [HttpGet]
        public async Task<JsonResult> GetLatestData(int id)
        {
            if (!User.HasClaim("Permission", "DOLLY_VIEW") && !User.IsInRole("Admin"))
            {
                return Json(new { error = "Yetkisiz eriþim" });
            }

            var sicakliklar = await _apiService.GetAsync<List<Sicaklik>>("api/Sicaklik/GetAll") ?? new List<Sicaklik>();
            var nemler = await _apiService.GetAsync<List<Nem>>("api/Nem/GetAll") ?? new List<Nem>();
            var gpsler = await _apiService.GetAsync<List<Gpsdatum>>("api/Gps/GetAll") ?? new List<Gpsdatum>();
            var durumlar = await _apiService.GetAsync<List<BosDolu>>("api/DoluBos/GetAll") ?? new List<BosDolu>();

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

        // Dashboard kartlarinin AJAX ile canli guncellenmesi icin her dolly'nin son degerleri
        [HttpGet]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> GetLatestAll()
        {
            if (!User.HasClaim("Permission", "DASHBOARD_VIEW") && !User.IsInRole("Admin"))
            {
                return Json(new { error = "Yetkisiz erisim" });
            }

            var snapshotTask = _apiService.GetAsync<LatestSnapshot>("api/Latest/GetAll");
            var openTask = _apiService.GetAsync<List<DollyRequestItemDto>>("api/DollyRequest/GetOpen");
            await Task.WhenAll(snapshotTask, openTask);

            var snapshot = await snapshotTask ?? new LatestSnapshot();
            var openRequests = await openTask ?? new List<DollyRequestItemDto>();
            var dollyIds = snapshot.Sicaklik.Select(x => x.DollyId)
                .Union(snapshot.Nem.Select(x => x.DollyId))
                .Union(snapshot.Gps.Select(x => x.DollyId))
                .Union(snapshot.BosDolu.Select(x => x.DollyId))
                .Union(openRequests.Select(x => x.DollyId));

            var result = dollyIds.Select(id =>
            {
                var s = snapshot.Sicaklik.FirstOrDefault(x => x.DollyId == id);
                var n = snapshot.Nem.FirstOrDefault(x => x.DollyId == id);
                var g = snapshot.Gps.FirstOrDefault(x => x.DollyId == id);
                var b = snapshot.BosDolu.FirstOrDefault(x => x.DollyId == id);
                var o = openRequests.Where(x => x.DollyId == id).ToList();
                return new
                {
                    dollyId = id,
                    sicaklik = s?.Sicaklik1,
                    nem = n?.Nem1,
                    lat = g?.Latitude,
                    lng = g?.Longitude,
                    isFull = b?.SensorDegeri ?? false,
                    updated = s?.Time?.ToString("dd.MM.yyyy HH:mm:ss"),
                    gpsTime = g?.Time?.ToString("dd.MM.yyyy HH:mm:ss"),
                    uldNo = string.Join(", ", o.Select(x => x.UldNo)),
                    sccCode = string.Join(", ", o.Where(x => !string.IsNullOrEmpty(x.SccCode)).Select(x => x.SccCode).Distinct())
                };
            }).ToList();

            return Json(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetChartData(int id, DateTime start, DateTime end)
        {
            if (!User.HasClaim("Permission", "DOLLY_VIEW") && !User.IsInRole("Admin"))
            {
                return Json(new { error = "Yetkisiz erisim" });
            }

            var rangeParams = new Dictionary<string, string>
            {
                { "dollyId", id.ToString() },
                { "start", start.ToString("yyyy-MM-ddTHH:mm:ss") },
                { "end", end.ToString("yyyy-MM-ddTHH:mm:ss") }
            };

            var tempTask = _apiService.GetAsync<List<Sicaklik>>("api/Sicaklik/GetAll", rangeParams);
            var humTask = _apiService.GetAsync<List<Nem>>("api/Nem/GetAll", rangeParams);
            await Task.WhenAll(tempTask, humTask);

            return Json(new
            {
                temps = (tempTask.Result ?? new List<Sicaklik>()).Where(x => x.Time.HasValue).OrderBy(x => x.Time)
                    .Select(x => new { t = x.Time!.Value.ToString("yyyy-MM-ddTHH:mm:ss"), v = x.Sicaklik1 }),
                hums = (humTask.Result ?? new List<Nem>()).Where(x => x.Time.HasValue).OrderBy(x => x.Time)
                    .Select(x => new { t = x.Time!.Value.ToString("yyyy-MM-ddTHH:mm:ss"), v = x.Nem1 })
            });
        }

        [HttpGet]
        public async Task<IActionResult> GetHistoryData(int id, DateTime? start, DateTime? end)
        {
            if (!User.HasClaim("Permission", "DOLLY_VIEW") && !User.IsInRole("Admin"))
            {
                return Json(new { error = "Yetkisiz eriþim" });
            }

            var queryParams = new Dictionary<string, string> { { "id", id.ToString() } };
            if (start.HasValue) queryParams.Add("start", start.Value.ToString("yyyy-MM-ddTHH:mm:ss"));
            if (end.HasValue) queryParams.Add("end", end.Value.ToString("yyyy-MM-ddTHH:mm:ss"));

            var data = await _apiService.GetAsync<List<GPSHistoryModel>>("api/Gps/GetHistoryData", queryParams) ?? new List<GPSHistoryModel>();
            return Json(data);
        }

        // ==========================================
        // OIDC / THY SSO ENTEGRASYON ALANI
        // ==========================================

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Home");
            }

            byte[] stateBytes = RandomNumberGenerator.GetBytes(32);
            byte[] nonceBytes = RandomNumberGenerator.GetBytes(16);

            string state = Convert.ToBase64String(stateBytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');
            string nonce = Convert.ToBase64String(nonceBytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');

            var clientId = _config["OidcSettings:ClientId"] ?? "";
            var redirectUri = _config["OidcSettings:RedirectUri"] ?? "";
            var authorizeUrl = _config["OidcSettings:AuthorizeUrl"] ?? "";
            var scope = _config["OidcSettings:Scope"] ?? "openid profile email";

            string authRedirectUrl = $"{authorizeUrl}?response_type=code" +
                                     $"&client_id={Uri.EscapeDataString(clientId)}" +
                                     $"&scope={Uri.EscapeDataString(scope)}" +
                                     $"&redirect_uri={Uri.EscapeDataString(redirectUri)}" +
                                     $"&state={Uri.EscapeDataString(state)}" +
                                     $"&nonce={Uri.EscapeDataString(nonce)}";

            return Redirect(authRedirectUrl);
        }

        [HttpGet]
        [Route("Callback")]
        [Route("Home/Callback")]
        [AllowAnonymous]
        public async Task<IActionResult> Callback(string code, string state)
        {
            string logPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "oidc_log.txt");
            void Log(string msg) => System.IO.File.AppendAllText(logPath, $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {msg}\n");

            Log("1. Callback tetiklendi. Code: " + code);

            if (string.IsNullOrEmpty(code))
            {
                Log("HATA: Code boþ döndü.");
                return RedirectToAction("AccessDenied", "Home");
            }

            OidcTokenResponseDto tokenResponse = null;
            try
            {
                using (var httpClient = new HttpClient())
                {
                    var tokenUrl = _config["OidcSettings:TokenUrl"];
                    var tokenParams = new Dictionary<string, string>
                    {
                        { "grant_type", "authorization_code" },
                        { "code", code },
                        { "client_id", _config["OidcSettings:ClientId"] ?? "" },
                        { "redirect_uri", _config["OidcSettings:RedirectUri"] ?? "" }
                    };

                    var requestContent = new FormUrlEncodedContent(tokenParams);

                    Log($"1.1 Token URL'ye Form post atýlýyor: {tokenUrl}");
                    var httpResponse = await httpClient.PostAsync(tokenUrl, requestContent);
                    var responseContent = await httpResponse.Content.ReadAsStringAsync();

                    Log($"1.2 HTTP Status: {(int)httpResponse.StatusCode} {httpResponse.StatusCode}");
                    Log($"1.3 Response Body: {responseContent}");

                    if (httpResponse.IsSuccessStatusCode)
                    {
                        var jsonObj = Newtonsoft.Json.Linq.JObject.Parse(responseContent);
                        string idToken = jsonObj["id_token"]?.ToString() ?? "";
                        string accessToken = jsonObj["access_token"]?.ToString() ?? "";

                        if (!string.IsNullOrEmpty(idToken))
                        {
                            tokenResponse = new OidcTokenResponseDto
                            {
                                IdToken = idToken,
                                AccessToken = accessToken
                            };
                            Log("1.4 IdToken ve AccessToken baþarýyla ayrýþtýrýldý.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log($"HATA Ex: {ex.Message}");
            }

            if (tokenResponse == null || string.IsNullOrEmpty(tokenResponse.IdToken))
            {
                Log("HATA: Token yanýtý parse edilemedi veya IdToken boþ.");
                return RedirectToAction("AccessDenied", "Home");
            }

            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(tokenResponse.IdToken);

            string username = jwtToken.Claims.FirstOrDefault(c =>
                c.Type == "preferred_username" ||
                c.Type == "unique_name" ||
                c.Type == "sub" ||
                c.Type == "username")?.Value ?? "";

            string email = jwtToken.Claims.FirstOrDefault(c => c.Type == "email")?.Value ?? "";

            Log($"2. Token decode edildi. Username: {username}, Email: {email}");

            var ssoUserDto = new { Username = username, Email = email };
            var userDetail = await _apiService.PostAsync<SsoUserResponseDto, object>("api/Users/get-or-create-sso-user", ssoUserDto);

            Log($"3. API UserDetail yanýtý: {(userDetail != null ? "Baþarýlý" : "NULL")}");

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userDetail?.UserId.ToString() ?? username),
                new Claim(ClaimTypes.Name, userDetail?.UserName ?? username),
                new Claim(ClaimTypes.Email, email),
                new Claim("JWToken", tokenResponse.AccessToken ?? "")
            };

            var adminUsers = _config.GetSection("OidcSettings:AdminUsers").Get<List<string>>() ?? new List<string>();

            bool isAdmin = adminUsers.Any(a => a.Equals(username, StringComparison.OrdinalIgnoreCase)) ||
                           (userDetail?.Roles != null && userDetail.Roles.Contains("Admin"));

            if (isAdmin)
            {
                // Admin Kullanýcýsýna TÜM SAYFA ÝZÝNLERÝ YÜKLENÝYOR
                claims.Add(new Claim(ClaimTypes.Role, "Admin"));
                claims.Add(new Claim("Permission", "DASHBOARD_VIEW"));
                claims.Add(new Claim("Permission", "DOLLY_VIEW"));
                claims.Add(new Claim("Permission", "DOLLY_EDIT"));
                claims.Add(new Claim("Permission", "USER_MANAGEMENT"));
                claims.Add(new Claim("Permission", "ROLE_MANAGEMENT"));

                Log($"4. ADMIN GÝRÝÞÝ: '{username}' kullanýcýsýna tüm sayfa yetkileri tanýmlandý.");
            }
            else
            {
                // --------------------------------------------------------------------------
                // VARSAYILAN (DEFAULT) YETKÝ: HÝÇBÝR YETKÝSÝ OLMASA DAHÝ DOLLY_VIEW VERÝLÝYOR
                // --------------------------------------------------------------------------
              
                claims.Add(new Claim("Permission", "DASHBOARD_VIEW")); // Ana sayfayý (Home/Index) görebilmesi için

                Log("4. Standart / Yeni Kullanýcý: Varsayýlan olarak 'DASHBOARD_VIEW' ve 'DOLLY_VIEW' yetkisi tanýmlandý.");

                // Eðer DB'de özel tanýmlanmýþ ek rolleri varsa yükle
                if (userDetail?.Roles != null)
                {
                    foreach (var roleName in userDetail.Roles)
                    {
                        claims.Add(new Claim(ClaimTypes.Role, roleName));
                        Log("Role eklendi: " + roleName);
                    }
                }

                // Eðer DB'de özel tanýmlanmýþ ekstra izinleri varsa yükle
                if (userDetail?.Permissions != null)
                {
                    foreach (var perm in userDetail.Permissions)
                    {
                        // Mükerrer (tekrar eden) claim eklememek için kontrol
                        if (!claims.Any(c => c.Type == "Permission" && c.Value == perm))
                        {
                            claims.Add(new Claim("Permission", perm));
                            Log("DB Permission eklendi: " + perm);
                        }
                    }
                }
            }

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity));

            Log("5. SignInAsync baþarýlý. Ana sayfaya yönlendiriliyor.");
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            if (Request.Cookies.ContainsKey("oidc_state"))
            {
                Response.Cookies.Delete("oidc_state");
            }

            string authorizeUrl = _config["OidcSettings:AuthorizeUrl"];
            string clientId = _config["OidcSettings:ClientId"];
            string redirectUri = _config["OidcSettings:RedirectUri"];
            string scope = _config["OidcSettings:Scope"] ?? "openid profile email";
            string state = Guid.NewGuid().ToString("N");

            Response.Cookies.Append("oidc_state", state, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                Expires = DateTimeOffset.Now.AddMinutes(5)
            });

            string ssoLoginUrl = $"{authorizeUrl}?response_type=code&client_id={clientId}&scope={Uri.EscapeDataString(scope)}&redirect_uri={Uri.EscapeDataString(redirectUri)}&state={state}";

            return Redirect(ssoLoginUrl);
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
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }

        [JsonProperty("id_token")]
        public string IdToken { get; set; }

        [JsonProperty("refresh_token")]
        public string RefreshToken { get; set; }

        [JsonProperty("token_type")]
        public string TokenType { get; set; }

        [JsonProperty("expires_in")]
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