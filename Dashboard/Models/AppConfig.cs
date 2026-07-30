using Microsoft.Extensions.Configuration;

namespace Dashboard.Models
{
    public static class AppConfig
    {
        public static IConfiguration Configuration { get; set; }

        public static string BaseUrl => Configuration?["ApiSettings:BaseUrl"];
    }
}