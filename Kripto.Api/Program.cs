using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Kripto.Api.Data;
using Microsoft.EntityFrameworkCore;
using Kripto.Api.Services;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(Options =>
{
    Options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});
// Controllers
builder.Services.AddControllers();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Kripto API",
        Version = "v1"
    });
});

// DbContext ve PostgreSQL bağlantısı
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// OKX WebSocket servisini singleton olarak ekle
var symbols = new List<string> { "BTC-USDT", "ETH-USDT", "SOL-USDT", "XRP-USDT", "DOGE-USDT" };
var okxService = new OkxWebSocketService(symbols);
builder.Services.AddSingleton(okxService);

// JWT (appsettings.json’da varsa)
var jwtKey = builder.Configuration["Jwt:Key"];
if (!string.IsNullOrWhiteSpace(jwtKey))
{
    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(o =>
        {
            o.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
            };
        });

    builder.Services.AddAuthorization();
}


var app = builder.Build();

app.UseCors();
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    // DİKKAT: buradaki yol ile JSON’un çıktığı yol birebir aynı olmalı!
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Kripto API v1");
});

// Middleware
if (!string.IsNullOrWhiteSpace(jwtKey))
{
    app.UseAuthentication();
    app.UseAuthorization();
}

app.UseWebSockets();

// WebSocket endpoint: Frontend'e anlık fiyat gönderimi
app.Map("/ws/prices", async context =>
{
    if (context.WebSockets.IsWebSocketRequest)
    {
        var ws = await context.WebSockets.AcceptWebSocketAsync();
        var okxService = app.Services.GetRequiredService<OkxWebSocketService>();

        while (ws.State == System.Net.WebSockets.WebSocketState.Open)
        {
            var prices = okxService.GetPrices();
            var json = System.Text.Json.JsonSerializer.Serialize(prices);
            var buffer = System.Text.Encoding.UTF8.GetBytes(json);
            await ws.SendAsync(buffer, System.Net.WebSockets.WebSocketMessageType.Text, true, CancellationToken.None);

            await Task.Delay(2000); // 2 saniyede bir fiyat gönder
        }
    }
    else
    {
        context.Response.StatusCode = 400;
    }
});

app.MapControllers();

// Health
app.MapGet("/health", () => Results.Ok(new { ok = true, ts = DateTimeOffset.UtcNow }));

// OKX WebSocket servisini başlat
var cts = new CancellationTokenSource();
_ = okxService.StartAsync(cts.Token);

app.Run();