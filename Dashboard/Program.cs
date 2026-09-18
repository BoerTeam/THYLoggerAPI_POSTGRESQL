using Dashboard.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.StaticFiles;

var builder = WebApplication.CreateBuilder(args);

// 1. Controller ve View Servisleri
builder.Services.AddControllersWithViews();

// 2. Cookie Authentication Kaydý
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Home/Login";
        options.AccessDeniedPath = "/Home/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
    });

// 2.1 Role ve Permission Tabanlý Policy Tanýmlarý (Tüm Controller'lar Ýçin)
builder.Services.AddAuthorization(options =>
{
    // Rol bazlý politika
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));

    // Ýzin (Permission) bazlý politikalar
    options.AddPolicy("DollyView", policy => policy.RequireClaim("Permission", "DOLLY_VIEW"));
    options.AddPolicy("DollyEdit", policy => policy.RequireClaim("Permission", "DOLLY_EDIT"));
    options.AddPolicy("DollyAdd", policy => policy.RequireClaim("Permission", "DOLLY_ADD"));
    options.AddPolicy("ExportExcel", policy => policy.RequireClaim("Permission", "EXPORT_EXCEL"));

    // Kullanýcý & Rol Yönetimi Politikasý: Hem Admin rolüne hem de ROLE_ADMIN iznine sahip olanlar girebilsin
    options.AddPolicy("UserView", policy =>
        policy.RequireAssertion(context =>
            context.User.IsInRole("Admin") ||
            context.User.HasClaim(c => c.Type == "Permission" && c.Value == "ROLE_ADMIN")
        )
    );
});

// 3. HttpContextAccessor - ApiService içinde Token okuyabilmek için þart
builder.Services.AddHttpContextAccessor();

// 4. Typed HttpClient ve ApiService Kaydý
builder.Services.AddHttpClient<IApiService, ApiService>(client =>
{
    var baseUrl = builder.Configuration["ApiSettings:BaseUrl"] ?? "https://localhost:44347";
    client.BaseAddress = new Uri(baseUrl.EndsWith("/") ? baseUrl : baseUrl + "/");
    client.Timeout = TimeSpan.FromSeconds(30);
});

var app = builder.Build();

// 5. HTTP Request Pipeline Yapýlandýrmasý
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
    app.UseHttpsRedirection();
}

// 6. Offline Harita (.tile) MIME Türü Tanýmlamasý
var provider = new FileExtensionContentTypeProvider();
provider.Mappings[".tile"] = "image/png";

app.UseStaticFiles(new StaticFileOptions
{
    ContentTypeProvider = provider
});

app.UseRouting();

// 7. Kimlik Doðrulama ve Yetkilendirme
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();