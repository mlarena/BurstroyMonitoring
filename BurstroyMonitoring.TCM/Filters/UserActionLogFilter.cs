using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using BurstroyMonitoring.Data;
using BurstroyMonitoring.Data.Models;
using BurstroyMonitoring.TCM.Attributes;
using System.Security.Claims;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace BurstroyMonitoring.TCM.Filters
{
    /// <summary>
    /// Фильтр для глобального логирования действий пользователей
    /// </summary>
    public class UserActionLogFilter : IAsyncActionFilter
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<UserActionLogFilter> _logger;
        private readonly Stopwatch _stopwatch;

        public UserActionLogFilter(ApplicationDbContext context, ILogger<UserActionLogFilter> logger)
        {
            _context = context;
            _logger = logger;
            _stopwatch = new Stopwatch();
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            _stopwatch.Start();

            // Проверяем, нужно ли пропустить логирование
            if (ShouldSkipLogging(context))
            {
                await next();
                return;
            }

            // Выполняем действие
            var resultContext = await next();
            _stopwatch.Stop();

            // Логируем только для авторизованных пользователей
            if (context.HttpContext.User.Identity?.IsAuthenticated == true)
            {
                try
                {
                    await LogUserAction(context, resultContext);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Ошибка при логировании действия пользователя");
                }
            }
        }

        private bool ShouldSkipLogging(ActionExecutingContext context)
        {
            var request = context.HttpContext.Request;
            var path = request.Path.Value ?? "";

            // 1. Пропускаем статические файлы и системные пути
            if (path.StartsWith("/css") || path.StartsWith("/js") || 
                path.StartsWith("/lib") || path.StartsWith("/images") ||
                path.StartsWith("/favicon.ico"))
            {
                return true;
            }

            // 2. Пропускаем запросы автодополнения и другие "шумные" эндпоинты
            if (path.Contains("/Autocomplete", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            // 3. Проверяем наличие атрибута SkipLogging на контроллере или методе
            var controllerHasSkip = context.Controller.GetType()
                .GetCustomAttributes(typeof(SkipLoggingAttribute), true)
                .Any();

            var actionHasSkip = context.ActionDescriptor.EndpointMetadata
                .Any(em => em.GetType() == typeof(SkipLoggingAttribute));

            if (controllerHasSkip || actionHasSkip)
                return true;

            // 4. ГЛАВНОЕ: Логируем только изменяющие действия (POST, PUT, DELETE)
            // GET запросы (просмотр страниц) обычно не требуют аудита действий, 
            // так как они не меняют состояние системы и создают много "шума".
            var method = request.Method.ToUpper();
            if (method == "GET")
            {
                return true;
            }

            return false;
        }
        private async Task LogUserAction(ActionExecutingContext context, ActionExecutedContext resultContext)
        {
            // Получаем информацию о пользователе
            var userIdClaim = context.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            int? userId = null;
            if (userIdClaim != null && int.TryParse(userIdClaim, out var parsed))
                userId = parsed;
            
            var userName = context.HttpContext.User.Identity?.Name ?? "Unknown";
            
            // Исключаем логирование для системных или неопределенных пользователей
            if (userName == "System" || userName == "Unknown")
            {
                return;
            }
            
            // Получаем название контроллера и действия
            var controllerName = context.RouteData.Values["controller"]?.ToString() ?? "Unknown";
            var actionName = context.RouteData.Values["action"]?.ToString() ?? "Unknown";            
            // Формируем детальную информацию
            var details = BuildActionDetails(context);
            
            // Получаем ID из маршрута
            int? targetId = null;
            if (context.RouteData.Values["id"] != null)
            {
                int.TryParse(context.RouteData.Values["id"]?.ToString(), out var id);
                targetId = id;
            }

            // Определяем успешность выполнения
            var isSuccess = resultContext.Exception == null || resultContext.ExceptionHandled;

            // Создаем запись лога
            var log = new AuditLog
            {
                Type = AuditLogType.Action,
                UserId = userId,
                UserName = userName,
                Action = $"{controllerName}.{actionName}",
                Details = details,
                TargetId = targetId,
                HttpMethod = context.HttpContext.Request.Method,
                Url = context.HttpContext.Request.Path.Value ?? "",
                IpAddress = context.HttpContext.Connection.RemoteIpAddress?.ToString(),
                UserAgent = context.HttpContext.Request.Headers["User-Agent"].ToString(),
                Timestamp = DateTime.UtcNow,
                IsSuccess = isSuccess,
                ExecutionTimeMs = _stopwatch.ElapsedMilliseconds
            };

            await _context.AuditLogs.AddAsync(log);
            await _context.SaveChangesAsync();
        }

        private string BuildActionDetails(ActionExecutingContext context)
        {
            var details = new List<string>();

            foreach (var param in context.ActionArguments)
            {
                // Пропускаем чувствительные данные
                if (param.Key.ToLower().Contains("password") || 
                    param.Key.ToLower().Contains("token") ||
                    param.Key.ToLower().Contains("secret"))
                {
                    details.Add($"{param.Key}=[HIDDEN]");
                    continue;
                }

                if (param.Value != null && param.Value.GetType().IsClass && 
                    param.Value.GetType() != typeof(string))
                {
                    details.Add($"{param.Key}=[{param.Value.GetType().Name}]");
                }
                else if (param.Value != null)
                {
                    var value = param.Value.ToString();
                    if (value?.Length > 50)
                    {
                        value = value.Substring(0, 47) + "...";
                    }
                    details.Add($"{param.Key}={value}");
                }
                else
                {
                    details.Add($"{param.Key}=null");
                }
            }

            return details.Count > 0 ? string.Join(", ", details) : "No parameters";
        }
    }
}