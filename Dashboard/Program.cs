using Dashboard.Models;
using Dashboard.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.StaticFiles; // 1. EKLEND�: Static files provider i�in gerekli
using System.Xml;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddHttpClient();
builder.Services.Configure<LdapAuthenticationOptions>(builder.Configuration.GetSection("Authentication:Ldap"));
builder.Services.AddScoped<ILdapAuthenticationService, LdapAuthenticationService>();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.Name = ".THYLogger.Session";
    options.IdleTimeout = TimeSpan.FromHours(8);
});
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
})
.AddCookie(options =>
{
    options.LoginPath = "/Home/Login";
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
    options.SlidingExpiration = true;
});
builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = options.DefaultPolicy;
});
AppConfig.Configuration = builder.Configuration;
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

// 2. G�NCELLEND�: .tile uzant�s�n� .NET'in tan�mas� i�in MIME ayar� eklendi
var provider = new FileExtensionContentTypeProvider();
provider.Mappings[".tile"] = "image/png";

app.UseStaticFiles(new StaticFileOptions
{
    ContentTypeProvider = provider
});

app.UseRouting();
app.UseSession();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Login}/{id?}");

app.Run();

public partial class Program
{
    public static string Service_Link = "";
    private static void readConfig()
    {
        string current = "";
        XmlTextReader xmlTextReader = new XmlTextReader("Webconfig.xml");

        while (xmlTextReader.Read())
        {
            if (xmlTextReader.NodeType == XmlNodeType.Element)
            {
                current = xmlTextReader.LocalName;
            }
            if (xmlTextReader.NodeType == XmlNodeType.Text)
            {
                if (current == "ServiceLink")
                {
                    Program.Service_Link = xmlTextReader.Value.Trim();
                }
            }
        }
        xmlTextReader.Close();
    }
}