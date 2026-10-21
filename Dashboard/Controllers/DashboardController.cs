using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Dashboard.Controllers
{
    /// <summary>
    /// SADECE yerel geliştirme içindir: THY SSO/LDAP'a ulaşılamayan makinede giriş yapmayı sağlar.
    /// İki şart birlikte sağlanmazsa 404 döner:
    ///   1) Ortam "Development" olmalı
    ///   2) appsettings.Development.json içinde DevLogin:Enabled = true olmalı
    /// Test ve canlı ortamda bu iki şart da yoktur, adres çalışmaz.
    /// </summary>
    [AllowAnonymous]
    public class DevAuthController : Controller
    {
        private readonly IHostEnvironment _env;
        private readonly IConfiguration _config;

        public DevAuthController(IHostEnvironment env, IConfiguration config)
        {
            _env = env;
            _config = config;
        }

        // /DevAuth/Login                                   -> Admin olarak giriş (tüm izinler)
        // /DevAuth/Login?admin=false&perms=DOLLY_VIEW      -> Admin olmayan kullanıcı, yalnızca verilen izinlerle
        [HttpGet]
        public async Task<IActionResult> Login(bool admin = true, string? perms = null)
        {
            if (!_env.IsDevelopment() || !_config.GetValue<bool>("DevLogin:Enabled"))
            {
                return NotFound();
            }

            // HomeController.Callback ile aynı claim yapısı
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, "1"),
                new(ClaimTypes.Name, admin ? "dev.admin" : "dev.user"),
                new(ClaimTypes.Email, "dev@localhost"),
                new("JWToken", "dev-local-token"),
                new("Permission", "DASHBOARD_VIEW")
            };

            if (admin)
            {
                claims.Add(new Claim(ClaimTypes.Role, "Admin"));
                claims.Add(new Claim("Permission", "DOLLY_VIEW"));
                claims.Add(new Claim("Permission", "DOLLY_EDIT"));
                claims.Add(new Claim("Permission", "USER_MANAGEMENT"));
                claims.Add(new Claim("Permission", "ROLE_MANAGEMENT"));
            }
            else if (!string.IsNullOrWhiteSpace(perms))
            {
                foreach (var p in perms.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                {
                    claims.Add(new Claim("Permission", p));
                }
            }

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identity));

            return RedirectToAction("Index", "Home");
        }
    }
}