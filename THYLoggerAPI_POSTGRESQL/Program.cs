using Microsoft.EntityFrameworkCore;
using Serilog;
using THYLoggerAPI_POSTGRESQL.Context;
using THYLoggerAPI_POSTGRESQL.Interceptors;
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

// Otomatik Database Migration (Eksik Tablolarý Otomatik Yükler/Günceller)
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        context.Database.Migrate();
        Console.WriteLine("--> Veritabaný ve tablolar baþarýyla kontrol edildi / güncellendi.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"--> Migration iþlemi sýrasýnda hata oluþtu: {ex.Message}");
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