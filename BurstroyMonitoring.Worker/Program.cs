using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using BurstroyMonitoring.Data;
using BurstroyMonitoring.Worker.Services;

namespace BurstroyMonitoring.Worker;

public class Program
{
    public static void Main(string[] args)
    {
        // Создание конфигурации
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddEnvironmentVariables()
            .Build();

        // Настройка логирования
        var filePath = configuration["Logging:FilePath"] ?? "logs/burstroy-worker-.log";
        var fileRetainedCount = configuration.GetValue<int?>("Logging:FileRetainedFileCountLimit") ?? 30;
        var consoleMinLevel = Enum.TryParse<LogEventLevel>(configuration["Logging:ConsoleMinimumLevel"], out var consoleLevel) 
            ? consoleLevel 
            : LogEventLevel.Information;
        var fileMinLevel = Enum.TryParse<LogEventLevel>(configuration["Logging:FileMinimumLevel"], out var fileLevel) 
            ? fileLevel 
            : LogEventLevel.Information;

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft.EntityFrameworkCore.Database.Command", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.Extensions.Http", LogEventLevel.Warning)
            .MinimumLevel.Override("System.Net.Http.HttpClient", LogEventLevel.Warning)
            .WriteTo.Console(
                restrictedToMinimumLevel: consoleMinLevel,
                outputTemplate: "{Timestamp:HH:mm:ss} [{Level:u3}] {Message:lj}{NewLine}{Exception}"
            )
            .WriteTo.File(
                path: filePath,
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: fileRetainedCount,
                restrictedToMinimumLevel: fileMinLevel,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}"
            )
            .CreateLogger();

        try
        {
            Log.Information("Starting BurstroyWorker application");
            CreateHostBuilder(args, configuration).Build().Run();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Application terminated unexpectedly");
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }

    public static IHostBuilder CreateHostBuilder(string[] args, IConfiguration configuration) =>
        Host.CreateDefaultBuilder(args)
            .UseSerilog()
            .ConfigureServices((hostContext, services) =>
            {
                // Регистрация контекста базы данных
                services.AddDbContext<ApplicationDbContext>(options =>
                {
                    options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"));
                    options.UseLoggerFactory(null);
                });

                // Регистрация сервисов с правильными временами жизни
                services.AddSingleton<IConfiguration>(configuration);
                services.AddSingleton<LoggerService>();
                services.AddSingleton<ConfigurationService>();
                services.AddSingleton<DatabaseService>();
                services.AddSingleton<DataProcessingService>();
                
                // Регистрация HttpClient для опроса датчиков без логирования
                services.AddHttpClient("SensorClient", client =>
                {
                    client.Timeout = TimeSpan.FromSeconds(
                        configuration.GetValue<int>("Polling:HttpTimeoutSeconds", 30));
                })
                .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
                });
                
                // SensorPollingService должен быть Transient или Scoped
                services.AddScoped<SensorPollingService>();
                
                // Worker должен быть Singleton
                services.AddSingleton<Worker>();
                
                // Регистрация IHostedService
                services.AddSingleton<IHostedService>(provider => provider.GetRequiredService<Worker>());
            });
}