using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Kripto.Api.Data;
using Microsoft.EntityFrameworkCore;
using Kripto.Api.Services;
using Microsoft.Extensions.Options;
using Npgsql;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
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
builder.Services.AddSingleton(sp => new OkxWebSocketService(symbols));

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

// --- Kalıcı çözüm: Otomatik Baseline + Migrate ---
await app.ApplyMigrationsWithBaselineAsync<AppDbContext>(
    baselineMigrationId: "20251003093431_InitialCreate" // İlk migrasyonunun ID’si
);
// ---------------------------------------------------

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
        var okxService = context.RequestServices.GetRequiredService<OkxWebSocketService>();

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
_ = app.Services.GetRequiredService<OkxWebSocketService>().StartAsync(cts.Token);

app.Run();


// ======================================================================
//  AKILLI MIGRATION YARDIMCISI  (Aynı dosyada tutabilir veya ayrı sınıfa alabilirsin)
// ======================================================================
public static class MigrationExtensions
{
    public static async Task ApplyMigrationsWithBaselineAsync<TContext>(
        this IApplicationBuilder app,
        string baselineMigrationId,        // ör: "20251003093431_InitialCreate"
        string? productVersion = null      // boş bırakılırsa otomatik bulunur
    ) where TContext : DbContext
    {
        using var scope = app.ApplicationServices.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TContext>();

        var conn = (NpgsqlConnection)db.Database.GetDbConnection();
        await conn.OpenAsync();

        // __EFMigrationsHistory var mı?
        const string historyExistsSql = @"SELECT to_regclass('__EFMigrationsHistory') IS NOT NULL;";
        var historyExistsObj = await new NpgsqlCommand(historyExistsSql, conn).ExecuteScalarAsync();
        var historyExists = historyExistsObj is bool b1 && b1;

        if (!historyExists)
        {
            // Kullanıcı tabloları var mı? (history hariç herhangi bir tablo)
            const string anyUserTableSql = @"
                SELECT EXISTS (
                    SELECT 1
                    FROM information_schema.tables
                    WHERE table_schema = 'public'
                      AND table_name <> '__EFMigrationsHistory'
                );";
            var anyUserTableObj = await new NpgsqlCommand(anyUserTableSql, conn).ExecuteScalarAsync();
            var anyUserTable = anyUserTableObj is bool b2 && b2;

            if (anyUserTable)
            {
                // History tablosunu oluştur
                const string createHistorySql = @"
                    CREATE TABLE IF NOT EXISTS ""__EFMigrationsHistory"" (
                        ""MigrationId"" character varying(150) NOT NULL,
                        ""ProductVersion"" character varying(32) NOT NULL,
                        CONSTRAINT ""PK___EFMigrationsHistory"" PRIMARY KEY (""MigrationId"")
                    );";
                await new NpgsqlCommand(createHistorySql, conn).ExecuteNonQueryAsync();

                // ProductVersion’ı otomatik bul (32 karaktere kes)
                productVersion ??= GetEfCoreProductVersion() ?? "9.0.0";
                if (productVersion.Length > 32) productVersion = productVersion[..32];

                const string insertBaselineSql = @"
                    INSERT INTO ""__EFMigrationsHistory"" (""MigrationId"", ""ProductVersion"")
                    SELECT @id, @ver
                    WHERE NOT EXISTS (
                        SELECT 1 FROM ""__EFMigrationsHistory"" WHERE ""MigrationId"" = @id
                    );";
                var cmd = new NpgsqlCommand(insertBaselineSql, conn);
                cmd.Parameters.AddWithValue("id", baselineMigrationId);
                cmd.Parameters.AddWithValue("ver", productVersion);
                await cmd.ExecuteNonQueryAsync();
            }
        }

        // Normal migrate
        await db.Database.MigrateAsync();
    }

    private static string? GetEfCoreProductVersion()
    {
        var asm = typeof(DbContext).Assembly; // Microsoft.EntityFrameworkCore
        var info = asm.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
                   ?? asm.GetName().Version?.ToString();
        return info?.Split('+')[0]; // "9.0.6+sha" -> "9.0.6"
    }
}