var builder = WebApplication.CreateBuilder(args);

// ------------------ Services ------------------

// CORS (Frontend: http://localhost:5173 -> Backend: bu proje)
builder.Services.AddCors(o => o.AddDefaultPolicy(p => p
    .WithOrigins(builder.Configuration.GetSection("AllowedOrigins").Get<string[]>()
                 ?? new[] { "http://localhost:5173" })
    .AllowAnyHeader()
    .AllowAnyMethod()
    .AllowCredentials()
));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();       // .NET 9 yerleşik OpenAPI
builder.Services.AddAuthorization(); // JWT ekleyince işe yarayacak (şimdilik dursun)

// ------------------ Pipeline ------------------

var app = builder.Build();

app.UseCors();

// İstersen https yönlendirmeyi şimdilik kapalı tut (dev ortamı için gerekmez)
// app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

// OpenAPI JSON + Swagger UI (tek yerde, bir kez)
app.MapOpenApi(); // -> /openapi/v1.json
app.UseSwaggerUI(o =>
{
    // ÖNEMLİ: v1/swagger.json DEĞİL; openapi/v1.json
    o.SwaggerEndpoint("/openapi/v1.json", "Kripto API");
});

// Basit sağlık kontrolü
app.MapGet("/health", () => Results.Ok(new { ok = true, ts = DateTimeOffset.UtcNow }));

app.Run();
