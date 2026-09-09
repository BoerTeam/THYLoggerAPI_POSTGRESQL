using Microsoft.EntityFrameworkCore;
using Serilog;
using THYLoggerAPI_POSTGRESQL.Context;
using THYLoggerAPI_POSTGRESQL.Interceptors;
using THYLoggerAPI_POSTGRESQL.Model;
using THYLoggerAPI_POSTGRESQL.Services;

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);

// 0. Serilog Yapýlandýrmasý
builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services));

// 1. Servis Kayýtlarý (Services)
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<AuditInterceptor>();

// Yetkilendirme ve Kimlik Doðrulama Servis Kayýtlarý
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<RoleService>();
builder.Services.AddScoped<PageService>();

// IoT ve Cihaz Takip Servis Kayýtlarý
builder.Services.AddScoped<DollyService>();
builder.Services.AddScoped<DoluBosService>();
builder.Services.AddScoped<GpsService>();
builder.Services.AddScoped<NemService>();
builder.Services.AddScoped<SicaklikService>();

// PostgreSQL Baðlantýsý
builder.Services.AddDbContext<ApplicationDbContext>((sp, options) =>
{
    var auditInterceptor = sp.GetRequiredService<AuditInterceptor>();

    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
           .AddInterceptors(auditInterceptor);
});

var app = builder.Build();

// Otomatik Database Migration ve Seed Verileri
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        context.Database.Migrate();

        // Temel Ýzinlerin (Permissions) Otomatik Eklenmesi
        if (!context.Permissions.Any())
        {
            context.Permissions.AddRange(
                new Permission { Name = "Dolly.Read", Description = "Dolly Cihazlarýný Görüntüleme", IsActive = true },
                new Permission { Name = "Dolly.Write", Description = "Dolly Cihazý Yönetimi", IsActive = true },
                new Permission { Name = "User.Manage", Description = "Kullanýcý ve Rol Yönetimi", IsActive = true }
            );
            context.SaveChanges();
            Console.WriteLine("--> Temel sistem izinleri (Permissions) veritabanýna eklendi.");
        }

        // Temel Sayfalarýn (Pages) Otomatik Eklenmesi
        if (!context.Pages.Any())
        {
            context.Pages.AddRange(
                new Page { Name = "Dolly Takip", Route = "/Dolly/Index", PermissionCode = "Dolly.Read", Icon = "fa-truck", Order = 1, IsActive = true },
                new Page { Name = "Kullanýcý Yönetimi", Route = "/Users/Index", PermissionCode = "User.Manage", Icon = "fa-users", Order = 2, IsActive = true }
            );
            context.SaveChanges();
            Console.WriteLine("--> Temel sayfa (Pages) verileri veritabanýna eklendi.");
        }

        Console.WriteLine("--> Veritabaný ve tablolar baþarýyla kontrol edildi / güncellendi.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"--> Migration/Seed iþlemi sýrasýnda hata oluþtu: {ex.Message}");
    }
}

// 2. HTTP Request Pipeline (Middleware) Yapýlandýrmasý
app.UseSerilogRequestLogging();
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "THYLogger API V1");
    c.RoutePrefix = "swagger";
});

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();