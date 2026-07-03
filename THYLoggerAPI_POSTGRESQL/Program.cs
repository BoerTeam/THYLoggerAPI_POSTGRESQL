using Microsoft.EntityFrameworkCore;
using THYLoggerAPI_POSTGRESQL.Context;

var builder = WebApplication.CreateBuilder(args);

// 1. Servis Kayýtlarý (Services)
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// PostgreSQL Baðlantýsý
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

// 2. HTTP Request Pipeline (Middleware) Yapýlandýrmasý

// 'if' kontrolünü kaldýrýyoruz ki Publish (Production) modunda da çalýþsýn
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");

    // c.RoutePrefix = "swagger"; // Bu aktifse: localhost:5000/swagger
    // c.RoutePrefix = string.Empty; // Bu aktifse: localhost:5000 (direkt ana dizin)
    c.RoutePrefix = "swagger";
});

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();