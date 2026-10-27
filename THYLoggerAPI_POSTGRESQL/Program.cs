using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Serilog;
using THYLoggerAPI_POSTGRESQL.Context;
using THYLoggerAPI_POSTGRESQL.Interceptors;
using THYLoggerAPI_POSTGRESQL.Model;
using THYLoggerAPI_POSTGRESQL.Services;
using Microsoft.EntityFrameworkCore.Diagnostics;

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

// Sensör Kalibrasyon Ayarlarýnýn appsettings.json Ýle Eþlenmesi
builder.Services.Configure<NemCalibrationOptions>(
    builder.Configuration.GetSection("SensorCalibration:Nem"));
builder.Services.Configure<SicaklikCalibrationOptions>(
    builder.Configuration.GetSection("SensorCalibration:Sicaklik"));
builder.Services.Configure<DoluBosDebounceOptions>(
    builder.Configuration.GetSection("DoluBosDebounce"));
// Yetkilendirme ve Kimlik Doðrulama Servis Kayýtlarý
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<RoleService>();
builder.Services.AddScoped<PageService>();
builder.Services.AddScoped<JwtTokenService>();

// IoT ve Cihaz Takip Servis Kayýtlarý
builder.Services.AddScoped<DollyService>();
builder.Services.AddScoped<DoluBosService>();
builder.Services.AddScoped<GpsService>();
builder.Services.AddScoped<NemService>();
builder.Services.AddScoped<SicaklikService>();
builder.Services.AddScoped<LatestService>();
builder.Services.AddScoped<AuditLogService>();
builder.Services.AddScoped<CoolDollyRequestService>();
builder.Services.AddScoped<ConfigService>();

// BPM entegrasyonu: Rate limit (servis mapping dokümaný, bölüm 9: dakikada 100 istek)
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("bpm-limit", opt =>
    {
        opt.PermitLimit = 100;
        opt.Window = TimeSpan.FromMinutes(1);
        opt.QueueLimit = 0;
    });
    options.OnRejected = async (ctx, ct) =>
    {
        var ex = CoolDollyException.RateLimited();
        ctx.HttpContext.Response.StatusCode = ex.StatusCode;
        await ctx.HttpContext.Response.WriteAsJsonAsync(
            THYLoggerAPI_POSTGRESQL.DTOs.CoolDollyResponse.Error(ex.Code, ex.Message), ct);
    };
});

// PostgreSQL Baðlantýsý
builder.Services.AddDbContext<ApplicationDbContext>((sp, options) =>
{
    var auditInterceptor = sp.GetRequiredService<AuditInterceptor>();

    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
           .AddInterceptors(auditInterceptor)
           .ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning));
});

var app = builder.Build();

// ==========================================
// AUTOMATIC DATABASE MIGRATION & SEED DATA
// ==========================================
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();

        // 1. Tablolar yoksa oluþturur, varsa eksik migrasyonlarý uygular
        context.Database.Migrate();

        // 2. Temel Ýzinlerin (Permissions) Eklemesi
        if (!context.Permissions.Any())
        {
            var p1 = new Permission { Name = "Dolly Görüntüleme", Code = "DollyView" };
            var p2 = new Permission { Name = "Dolly Düzenleme", Code = "DollyEdit" };
            var p3 = new Permission { Name = "Excel Çýktýsý Alma", Code = "ExportExcel" };
            var p4 = new Permission { Name = "Kullanýcý Yönetimi", Code = "UserManagement" };

            context.Permissions.AddRange(p1, p2, p3, p4);
            context.SaveChanges();
            Console.WriteLine("--> Temel sistem izinleri (Permissions) eklendi.");
        }

        // 3. Temel Rollerin (Roles) Eklenmesi
        if (!context.Roles.Any())
        {
            var adminRole = new Role { Name = "Admin" };
            var userRole = new Role { Name = "User" };

            context.Roles.AddRange(adminRole, userRole);
            context.SaveChanges();
            Console.WriteLine("--> Temel roller (Admin, User) eklendi.");

            // Admin Rolüne Tüm Ýzinlerin Baðlanmasý
            var allPermissions = context.Permissions.ToList();
            foreach (var perm in allPermissions)
            {
                context.RolePermissions.Add(new RolePermission { RoleId = adminRole.Id, PermissionId = perm.Id });
            }
            context.SaveChanges();
            Console.WriteLine("--> Admin rolüne tüm izinler baþarýyla tanýmlandý.");
        }

        // 4. Varsayýlan Admin Kullanýcýsýnýn Eklenmesi
        if (!context.Users.Any())
        {
            var adminUser = new User
            {
                UserName = "admin",
                Email = "admin@boer.com.tr",
                PasswordHash = "SYSTEM_INITIAL_SEED_HASH"
            };

            context.Users.Add(adminUser);
            context.SaveChanges();

            // Admin Kullanýcýsýna Admin Rolünün Atanmasý
            var adminRole = context.Roles.FirstOrDefault(r => r.Name == "Admin");
            if (adminRole != null)
            {
                context.UserRoles.Add(new UserRole { UserId = adminUser.Id, RoleId = adminRole.Id });
                context.SaveChanges();
            }
            Console.WriteLine("--> Varsayýlan admin kullanýcýsý eklendi ve Admin rolü atandý.");
        }

        // 5. Temel Sayfalarýn (Pages) Eklenmesi
        if (!context.Pages.Any())
        {
            context.Pages.AddRange(
                new Page { Name = "Dolly Takip", Route = "/Home/Index", PermissionCode = "DollyView" },
                new Page { Name = "Kullanýcý Yönetimi", Route = "/Users/Index", PermissionCode = "UserManagement" }
            );
            context.SaveChanges();
            Console.WriteLine("--> Temel sayfa (Pages) verileri eklendi.");
        }

        // 6. Denetim Kayýtlarý menü sayfasý (var olan kurulumlarda da eklenir; yalnýzca Admin görür)
        if (!context.Pages.Any(p => p.Route == "/AuditLog/Index"))
        {
            var nextOrder = context.Pages.Any() ? context.Pages.Max(p => p.Order) + 1 : 1;
            context.Pages.Add(new Page { Name = "Denetim Kayýtlarý", Route = "/AuditLog/Index", PermissionCode = "AdminOnly", Icon = "shield", Order = nextOrder });
            context.SaveChanges();
            Console.WriteLine("--> Denetim Kayýtlarý menü sayfasý eklendi.");
        }

        // 7. Dolly Talepleri menü sayfasý (var olan kurulumlarda da eklenir; DOLLY_VIEW izni olanlar ve Admin görür)
        if (!context.Pages.Any(p => p.Route == "/DollyRequest/Index"))
        {
            var nextOrder = context.Pages.Any() ? context.Pages.Max(p => p.Order) + 1 : 1;
            context.Pages.Add(new Page { Name = "Dolly Talepleri", Route = "/DollyRequest/Index", PermissionCode = "DOLLY_VIEW", Icon = "list", Order = nextOrder });
            context.SaveChanges();
            Console.WriteLine("--> Dolly Talepleri menü sayfasý eklendi.");
        }

        // 8. Cihaz Konfigurasyonu ve Rapor menü sayfalarý (yalnýzca Admin)
        foreach (var (name, route, icon) in new[]
        {
            ("Cihaz Konfigürasyonu", "/Config/Index", "settings"),
            ("Konfigürasyon Raporu", "/Config/Report", "activity")
        })
        {
            if (!context.Pages.Any(p => p.Route == route))
            {
                var nextOrder = context.Pages.Any() ? context.Pages.Max(p => p.Order) + 1 : 1;
                context.Pages.Add(new Page { Name = name, Route = route, PermissionCode = "AdminOnly", Icon = icon, Order = nextOrder });
                context.SaveChanges();
                Console.WriteLine($"--> {name} menü sayfasý eklendi.");
            }
        }

        Console.WriteLine("--> Veritabaný, migrasyonlar ve varsayýlan veriler baþarýyla kontrol edildi.");
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
app.UseRateLimiter();
app.UseAuthorization();
app.MapControllers();

app.Run();