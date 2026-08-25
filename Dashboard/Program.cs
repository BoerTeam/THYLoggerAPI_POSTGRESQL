using Dashboard.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.StaticFiles; // 1. EKLEND�: Static files provider i�in gerekli
using System.Xml;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddHttpClient();
var oidcClientId = builder.Configuration["Authentication:Oidc:ClientId"];
if (string.IsNullOrWhiteSpace(oidcClientId) || oidcClientId.StartsWith("<set-via-env:", StringComparison.Ordinal))
{
    throw new InvalidOperationException("Authentication:Oidc:ClientId must be configured for THY OIDC login.");
}
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
})
.AddCookie(options =>
{
    options.LoginPath = "/Home/Login";
})
.AddOpenIdConnect(options =>
{
    options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.MetadataAddress = builder.Configuration["Authentication:Oidc:MetadataAddress"] ?? "https://oidctest.thy.com/idp/.well-known/openid-configurations";
    options.ClientId = oidcClientId;
    options.CallbackPath = builder.Configuration["Authentication:Oidc:CallbackPath"] ?? "/callback";
    options.ResponseType = "code";
    options.UsePkce = true;
    options.SaveTokens = true;
    options.GetClaimsFromUserInfoEndpoint = true;

    options.Scope.Clear();
    options.Scope.Add("openid");
    options.Scope.Add("profile");
    options.Scope.Add("email");
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