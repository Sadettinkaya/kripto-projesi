using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Kripto.Api.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

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

app.MapControllers();

// Health
app.MapGet("/health", () => Results.Ok(new { ok = true, ts = DateTimeOffset.UtcNow }));

app.Run();