using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using THYLoggerAPI_POSTGRESQL.DTOs;
using THYLoggerAPI_POSTGRESQL.Services;

namespace THYLoggerAPI_POSTGRESQL.Filters
{
    /// <summary>
    /// BPM'den gelen isteklerde "X-Api-Key" header'ını appsettings'teki BpmIntegration:ApiKey ile karşılaştırır.
    /// Authorization filter olduğu için model binding'den ÖNCE çalışır; yetkisiz çağrı hiçbir şey öğrenemez.
    /// Anahtar tanımlı değilse hiçbir istek geçmez (fail closed).
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class ApiKeyAuthAttribute : Attribute, IAsyncAuthorizationFilter
    {
        private const string HeaderName = "X-Api-Key";

        public Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            var services = context.HttpContext.RequestServices;
            var config = services.GetRequiredService<IConfiguration>();
            var logger = services.GetRequiredService<ILogger<ApiKeyAuthAttribute>>();

            var expected = config["BpmIntegration:ApiKey"];

            if (string.IsNullOrWhiteSpace(expected))
            {
                logger.LogError("BpmIntegration:ApiKey tanımlı değil, BPM istekleri reddediliyor.");
                context.Result = Unauthorized();
                return Task.CompletedTask;
            }

            if (!context.HttpContext.Request.Headers.TryGetValue(HeaderName, out var provided) ||
                !CryptographicOperations.FixedTimeEquals(
                    Encoding.UTF8.GetBytes(provided.ToString()),
                    Encoding.UTF8.GetBytes(expected)))
            {
                logger.LogWarning("Geçersiz veya eksik API key. IP: {Ip}", context.HttpContext.Connection.RemoteIpAddress);
                context.Result = Unauthorized();
            }

            return Task.CompletedTask;
        }

        private static ObjectResult Unauthorized()
        {
            var ex = CoolDollyException.Unauthorized();
            return new ObjectResult(CoolDollyResponse.Error(ex.Code, ex.Message))
            {
                StatusCode = ex.StatusCode
            };
        }
    }
}
