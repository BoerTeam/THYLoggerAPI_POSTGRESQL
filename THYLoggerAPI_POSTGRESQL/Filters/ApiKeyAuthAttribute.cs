using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using THYLoggerAPI_POSTGRESQL.DTOs;

namespace THYLoggerAPI_POSTGRESQL.Filters
{
    /// <summary>
    /// BPM'den gelen isteklerde HTTP Basic Authentication doğrulaması yapar (Doküman v1.1 Bölüm 3 & 9).
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class BpmBasicAuthAttribute : Attribute, IAsyncAuthorizationFilter
    {
        public Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            var services = context.HttpContext.RequestServices;
            var config = services.GetRequiredService<IConfiguration>();
            var logger = services.GetRequiredService<ILogger<BpmBasicAuthAttribute>>();

            var expectedUser = config["BpmIntegration:Username"];
            var expectedPass = config["BpmIntegration:Password"];

            if (string.IsNullOrWhiteSpace(expectedUser) || string.IsNullOrWhiteSpace(expectedPass))
            {
                logger.LogError("BpmIntegration:Username veya Password tanımlı değil, BPM istekleri reddediliyor.");
                context.Result = Unauthorized(context.HttpContext);
                return Task.CompletedTask;
            }

            if (!context.HttpContext.Request.Headers.TryGetValue("Authorization", out var authHeaderValue) ||
                !AuthenticationHeaderValue.TryParse(authHeaderValue, out var authHeader) ||
                !string.Equals(authHeader.Scheme, "Basic", StringComparison.OrdinalIgnoreCase) ||
                string.IsNullOrWhiteSpace(authHeader.Parameter))
            {
                logger.LogWarning("Eksik veya geçersiz Basic Auth header. IP: {Ip}", context.HttpContext.Connection.RemoteIpAddress);
                context.Result = Unauthorized(context.HttpContext);
                return Task.CompletedTask;
            }

            try
            {
                var credentialBytes = Convert.FromBase64String(authHeader.Parameter);
                var credentials = Encoding.UTF8.GetString(credentialBytes).Split(':', 2);

                if (credentials.Length != 2 ||
                    !FixedTimeEquals(credentials[0], expectedUser) ||
                    !FixedTimeEquals(credentials[1], expectedPass))
                {
                    logger.LogWarning("Hatalı Basic Auth kullanıcı adı/şifre. IP: {Ip}", context.HttpContext.Connection.RemoteIpAddress);
                    context.Result = Unauthorized(context.HttpContext);
                }
            }
            catch (FormatException)
            {
                logger.LogWarning("Base64 formatı bozuk Basic Auth header. IP: {Ip}", context.HttpContext.Connection.RemoteIpAddress);
                context.Result = Unauthorized(context.HttpContext);
            }

            return Task.CompletedTask;
        }

        private static bool FixedTimeEquals(string a, string b) =>
            CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(a), Encoding.UTF8.GetBytes(b));

        private static ObjectResult Unauthorized(HttpContext httpContext)
        {
            httpContext.Response.Headers["WWW-Authenticate"] = "Basic realm=\"CoolDollyAPI\"";
            return new ObjectResult(new[]
            {
                CoolDollyResponse.Error("AUTH-401", "Yetkisiz erişim: Basic Auth bilgileri eksik veya hatalı")
            })
            {
                StatusCode = StatusCodes.Status401Unauthorized
            };
        }
    }
}