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
builder.Services.AddScoped<JwtTokenService>();

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
                PasswordHash = "SYSTEM_INITIAL_SEED_HASH" // PasswordHash IsRequired olduðu için dolduruldu
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
app.UseAuthorization();
app.MapControllers();

app.Run();