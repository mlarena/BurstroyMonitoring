using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using BurstroyMonitoring.Data;
using BurstroyMonitoring.Api.Services;
using System.Security.Claims;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Настройка Serilog для логирования в консоль
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container.
builder.Services.AddControllers();

// Настройка контекста базы данных
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

// Регистрация сервисов
builder.Services.AddScoped<IAuthService, AuthService>();

// Настройка аутентификации JWT
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? "YourSecretKeyHere-MakeItLongEnoughForSecurity"))
        };
    });

builder.Services.AddAuthorization();

// Шаг 1: Базовая настройка Swagger без параметров
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    // Шаг 1: Включение Swagger UI
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
