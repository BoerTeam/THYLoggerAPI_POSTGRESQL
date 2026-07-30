using Microsoft.EntityFrameworkCore;
using Serilog;
using THYLoggerAPI_POSTGRESQL.Context;
using THYLoggerAPI_POSTGRESQL.Interceptors;

var builder = WebApplication.CreateBuilder(args);

// 0. Serilog Yapýlandýrmasý (appsettings.json üzerindeki "Serilog" bloðunu okur)
builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services));

// 1. Servis Kayýtlarý (Services)
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<AuditInterceptor>();
// PostgreSQL Baðlantýsý
builder.Services.AddDbContext<ApplicationDbContext>((sp, options) =>
{
    var auditInterceptor = sp.GetRequiredService<AuditInterceptor>();

    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
           .AddInterceptors(auditInterceptor); 
});

var app = builder.Build();

// 2. HTTP Request Pipeline (Middleware) Yapýlandýrmasý

// API'ye gelen tüm istekleri (Status 200, 400, 500 vb.) otomatik loglar
app.UseSerilogRequestLogging();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
    c.RoutePrefix = "swagger";
});

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();