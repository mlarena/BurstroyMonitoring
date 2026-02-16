using Microsoft.EntityFrameworkCore;
using BurstroyMonitoring.Data;
using BurstroyMonitoring.TCM.Services;
using Serilog;
using System.Runtime.InteropServices;

using System.Globalization;

// Установите инвариантную культуру для всего приложения
var cultureInfo = new CultureInfo("en-US");
CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

var builder = WebApplication.CreateBuilder(args);

// Конфигурация для Linux
if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
{
    Environment.SetEnvironmentVariable("DOTNET_SYSTEM_GLOBALIZATION_INVARIANT", "false");
    Environment.SetEnvironmentVariable("LC_ALL", "en_US.UTF-8");
    Environment.SetEnvironmentVariable("LANG", "en_US.UTF-8");
}

// Настройка Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.WithProperty("Platform", RuntimeInformation.OSDescription)
    .CreateLogger();

builder.Host.UseSerilog();

try
{
    Log.Information("Starting Burstroy Monitoring UI on {Platform}", 
        RuntimeInformation.OSDescription);

    // Добавление сервисов
    builder.Services.AddControllersWithViews();

    // Поддержка сессий:
    builder.Services.AddDistributedMemoryCache();
    builder.Services.AddSession(options =>
    {
        options.IdleTimeout = TimeSpan.FromMinutes(30);
        options.Cookie.HttpOnly = true;
        options.Cookie.IsEssential = true;
        options.Cookie.Name = "BurstroyMonitoring.Session";
    });

    // Регистрация сервиса экспорта
    builder.Services.AddScoped<IExportService, ExportService>();

    // Настройка контекста базы данных
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    
    if (string.IsNullOrEmpty(connectionString))
    {
        Log.Warning("Connection string is not configured. Using development connection.");
        connectionString = "Host=localhost;Database=sensordb_new;Username=postgres;Password=12345678";
    }
    
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseNpgsql(connectionString));

    var app = builder.Build();

    // Настройка конвейера запросов
    if (app.Environment.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
    }
    else
    {
        app.UseExceptionHandler("/Home/Error");
        app.UseHsts();
    }
    app.UseSession();
    // Проверка подключения к БД (БЕЗ миграций)
    try
    {
        using var scope = app.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        // УБРАН вызов MigrateAsync() - миграции не применяются автоматически
        // Вместо этого просто проверяем подключение
        
        var canConnect = await dbContext.Database.CanConnectAsync();
        
        if (canConnect)
        {
            Log.Information("Successfully connected to the database: {Database}", 
                dbContext.Database.GetDbConnection().Database);
        }
        else
        {
            Log.Error("Could not connect to the database");
            // В Production можно решить, продолжать ли работу без БД
            if (!app.Environment.IsDevelopment())
            {
                Log.Warning("Application will start in read-only mode without database");
            }
        }
    }
    catch (Exception ex)
    {
        Log.Error(ex, "Error connecting to database");
        
        if (!app.Environment.IsDevelopment())
        {
            Log.Warning("Application will start without database connection");
        }
        else
        {
            throw;
        }
    }

    app.UseStaticFiles();
    app.UseRouting();
    app.UseAuthorization();

    app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");

    // Health check endpoint
    app.MapGet("/health", async (ApplicationDbContext dbContext) =>
    {
        try
        {
            var canConnect = await dbContext.Database.CanConnectAsync();
            return Results.Ok(new 
            { 
                status = canConnect ? "healthy" : "degraded",
                database = canConnect ? "connected" : "disconnected",
                timestamp = DateTime.UtcNow,
                platform = RuntimeInformation.OSDescription
            });
        }
        catch
        {
            return Results.Ok(new 
            { 
                status = "unhealthy",
                database = "error",
                timestamp = DateTime.UtcNow
            });
        }
    });

    Log.Information("Application configured successfully. Starting web server...");
   
    
    await app.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
    throw;
}
finally
{
    Log.CloseAndFlush();
}