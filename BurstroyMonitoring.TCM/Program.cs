using Microsoft.EntityFrameworkCore;
using BurstroyMonitoring.Data;
using BurstroyMonitoring.TCM.Services;
using Serilog;
using System.Runtime.InteropServices;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Text.Json.Serialization;
using BurstroyMonitoring.TCM.Filters;
using System.Globalization;
using BurstroyMonitoring.Data.Models;
using Microsoft.AspNetCore.Mvc;

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

    // ВАЖНО: Добавляем HttpContextAccessor ПЕРЕД регистрацией DbContext и фильтров
    builder.Services.AddHttpContextAccessor();

    // Настройка контекста базы данных с правильной передачей HttpContextAccessor
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    
    if (string.IsNullOrEmpty(connectionString))
    {
        Log.Warning("Connection string is not configured. Using development connection.");
        connectionString = "Host=localhost;Database=sensordb_new;Username=postgres;Password=12345678";
    }
    
   // Регистрация DbContext с IHttpContextAccessor и ILogger
    builder.Services.AddDbContext<ApplicationDbContext>((serviceProvider, options) =>
    {
        options.UseNpgsql(connectionString);
        if (builder.Environment.IsDevelopment())
        {
            options.EnableSensitiveDataLogging();
            options.EnableDetailedErrors();
        }
    }, ServiceLifetime.Scoped);

    // Явная регистрация фабрики
    builder.Services.AddScoped<ApplicationDbContext>(serviceProvider =>
    {
        var options = serviceProvider.GetRequiredService<DbContextOptions<ApplicationDbContext>>();
        var httpContextAccessor = serviceProvider.GetService<IHttpContextAccessor>();
        var logger = serviceProvider.GetRequiredService<ILogger<ApplicationDbContext>>();
        
        return new ApplicationDbContext(options, httpContextAccessor!, logger);
    });

    // ИСПРАВЛЕНИЕ 3: Регистрируем фильтр ПОСЛЕ регистрации DbContext
    builder.Services.AddScoped<UserActionLogFilter>();

    // Добавление сервисов MVC с глобальным фильтром
    builder.Services.AddControllersWithViews(options =>
    {
        // Добавляем фильтр логирования глобально
        options.Filters.Add<UserActionLogFilter>();
        
        // Добавляем обработку антифорджери токенов
        options.Filters.Add<AutoValidateAntiforgeryTokenAttribute>();
    })
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

    // Поддержка сессий:
    builder.Services.AddDistributedMemoryCache();
    builder.Services.AddSession(options =>
    {
        options.IdleTimeout = TimeSpan.FromMinutes(30);
        options.Cookie.HttpOnly = true;
        options.Cookie.IsEssential = true;
        options.Cookie.Name = "BurstroyMonitoring.Session";
    });

    // Добавляем Antiforgery с правильной настройкой для HTTP/HTTPS
    builder.Services.AddAntiforgery(options => 
    {
        options.HeaderName = "X-CSRF-TOKEN";
        options.Cookie.Name = "AntiForgeryCookie";
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    });

    // Добавляем CORS
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowAll",
            builder =>
            {
                builder.AllowAnyOrigin()
                       .AllowAnyMethod()
                       .AllowAnyHeader();
            });
    });

    // Регистрация сервисов
    builder.Services.AddScoped<IExportService, ExportService>();
    builder.Services.AddScoped<IAuthService, AuthService>();
    builder.Services.AddScoped<IWebPartService, WebPartService>();
    builder.Services.AddSingleton<RtspStreamService>();
    builder.Services.AddScoped<IDatabaseService, DatabaseService>();
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
            
            // Добавляем обработку события, когда токен не валиден
            options.Events = new JwtBearerEvents
            {
                OnChallenge = context =>
                {
                    context.HandleResponse();
                    context.Response.Redirect("/Auth/Login");
                    return Task.CompletedTask;
                },
                OnForbidden = context =>
                {
                    context.Response.Redirect("/Auth/Login");
                    return Task.CompletedTask;
                }
            };
        });

    builder.Services.AddAuthorization();

    // Добавляем Swagger (опционально)
    builder.Services.AddEndpointsApiExplorer();

    var app = builder.Build();

    // Настройка миграций базы данных и создание суперпользователя
    using (var scope = app.Services.CreateScope())
    {
        try
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            
            // Проверяем подключение к БД
            var canConnect = await dbContext.Database.CanConnectAsync();
            
            if (canConnect)
            {
                Log.Information("Successfully connected to the database: {Database}", 
                    dbContext.Database.GetDbConnection().Database);
                
                // ИСПРАВЛЕНИЕ 4: Применяем миграции для создания таблицы AuditLogs
                await dbContext.Database.MigrateAsync();
                
                // Создаём таблицу web_parts если не существует
                await dbContext.Database.ExecuteSqlRawAsync(@"
                    CREATE TABLE IF NOT EXISTS public.""WebParts"" (
                        id          SERIAL PRIMARY KEY,
                        title       VARCHAR(200) NOT NULL DEFAULT '',
                        type        INTEGER NOT NULL DEFAULT 0,
                        position_x  INTEGER NOT NULL DEFAULT 0,
                        position_y  INTEGER NOT NULL DEFAULT 0,
                        width       INTEGER NOT NULL DEFAULT 6,
                        height      INTEGER NOT NULL DEFAULT 4,
                        settings    TEXT NOT NULL DEFAULT '{{}}',
                        data        TEXT NOT NULL DEFAULT '{{}}',
                        user_id     INTEGER NOT NULL DEFAULT 0
                    )
                ");

                // Создаем суперпользователя, если его нет
                var authService = scope.ServiceProvider.GetRequiredService<IAuthService>();
                
                if (!await dbContext.Users.AnyAsync(u => u.UserName == "su"))
                {
                    var (hash, salt) = authService.HashPassword("su");
                    
                    var superUser = new User
                    {
                        UserName = "su",
                        PasswordHash = hash,
                        Salt = salt,
                        Role = "Admin"
                    };
                    
                    dbContext.Users.Add(superUser);
                    await dbContext.SaveChangesAsync();
                    Log.Information("Superuser 'su' created successfully");
                }
                
   
            }
            else
            {
                Log.Error("Could not connect to the database");
                if (!app.Environment.IsDevelopment())
                {
                    Log.Warning("Application will start in read-only mode without database");
                }
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error during database initialization");
            
            if (!app.Environment.IsDevelopment())
            {
                Log.Warning("Application will start without database connection");
            }
            else
            {
                throw;
            }
        }
    }

    // Настройка конвейера запросов
    if (app.Environment.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
    }
    else
    {
        app.UseExceptionHandler("/Home/Error");
        app.UseHsts();
        app.UseHttpsRedirection();
    }

    app.UseStaticFiles();
    app.UseRouting();
    app.UseCors("AllowAll");
    
    // ИСПРАВЛЕНИЕ 6: Session ДО Authentication
    app.UseSession();

    // Middleware для добавления токена из сессии в заголовок Authorization
    app.Use(async (context, next) =>
    {
        var token = context.Session.GetString("JWToken");
        if (!string.IsNullOrEmpty(token))
        {
            context.Request.Headers["Authorization"] = "Bearer " + token;
        }
        await next();
    });

    app.UseAuthentication();
    app.UseAuthorization();

    // Middleware для обработки маршрутов и редиректов
    app.Use(async (context, next) =>
    {
        // Пропускаем запросы к статическим файлам
        if (context.Request.Path.StartsWithSegments("/css") ||
            context.Request.Path.StartsWithSegments("/js") ||
            context.Request.Path.StartsWithSegments("/lib") ||
            context.Request.Path.StartsWithSegments("/images"))
        {
            await next();
            return;
        }

        // Обработка пустого маршрута Auth
        if (context.Request.Path.Equals("/Auth") || 
            context.Request.Path.Equals("/Auth/"))
        {
            context.Response.Redirect("/Auth/Login");
            return;
        }

        // Проверяем, является ли запрос POST запросом на Logout
        bool isLogoutPost = context.Request.Method == "POST" && 
                            context.Request.Path.Equals("/Auth/Logout");

        // Если пользователь не авторизован и пытается получить доступ не к Auth контроллеру
        if (!context.User.Identity?.IsAuthenticated == true && 
            !context.Request.Path.StartsWithSegments("/Auth") &&
            !isLogoutPost)
        {
            context.Response.Redirect("/Auth/Login");
            return;
        }

        // Если пользователь авторизован и пытается получить доступ к Auth контроллеру (кроме Logout)
        if (context.User.Identity?.IsAuthenticated == true && 
            context.Request.Path.StartsWithSegments("/Auth") &&
            !context.Request.Path.Equals("/Auth/Logout"))
        {
            context.Response.Redirect("/Home/Index");
            return;
        }

        await next();
    });

    // Middleware для обработки 404 ошибок
    app.Use(async (context, next) =>
    {
        await next();
        
        if (context.Response.StatusCode == 404 && !context.Response.HasStarted)
        {
            if (!context.User.Identity?.IsAuthenticated == true)
            {
                context.Response.Redirect("/Auth/Login");
            }
            else if (context.User.Identity?.IsAuthenticated == true)
            {
                context.Response.Redirect("/Home/Index");
            }
        }
    });

    // Настройка маршрутов
    app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");

    // Обработка корневого маршрута
    app.MapGet("/", context =>
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            context.Response.Redirect("/Home/Index");
        }
        else
        {
            context.Response.Redirect("/Auth/Login");
        }
        return Task.CompletedTask;
    });

    // Health check endpoint
    app.MapGet("/health", async (ApplicationDbContext dbContext) =>
    {
        try
        {
            var canConnect = await dbContext.Database.CanConnectAsync();
            
            // ИСПРАВЛЕНИЕ 7: Проверяем работу логирования
            var auditLogsCount = 0;
            if (canConnect)
            {
                auditLogsCount = await dbContext.AuditLogs.CountAsync();
            }
            
            return Results.Ok(new 
            { 
                status = canConnect ? "healthy" : "degraded",
                database = canConnect ? "connected" : "disconnected",
                authentication = "enabled",
                auditLogsCount = auditLogsCount,
                timestamp = DateTime.UtcNow,
                platform = RuntimeInformation.OSDescription
            });
        }
        catch (Exception ex)
        {
            return Results.Ok(new 
            { 
                status = "unhealthy",
                database = "error",
                authentication = "enabled",
                error = ex.Message,
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