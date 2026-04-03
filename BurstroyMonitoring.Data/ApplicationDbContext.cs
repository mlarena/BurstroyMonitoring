using Microsoft.EntityFrameworkCore;
using BurstroyMonitoring.Data.Models;
using Microsoft.EntityFrameworkCore.ChangeTracking; 
using System.Text.Json;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging; // Добавлено для ILogger

namespace BurstroyMonitoring.Data
{
    public class ApplicationDbContext : DbContext
    {
        private readonly IHttpContextAccessor? _httpContextAccessor;
        private readonly ILogger<ApplicationDbContext>? _logger;

        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options, 
            IHttpContextAccessor httpContextAccessor,
            ILogger<ApplicationDbContext> logger)
            : base(options)
        {
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Основные таблицы
        public DbSet<MonitoringPost> MonitoringPosts { get; set; }
        public DbSet<SensorType> SensorTypes { get; set; }
        public DbSet<Sensor> Sensors { get; set; }
        
        // Данные телеметрии
        public DbSet<DSPDData> DSPDData { get; set; }
        public DbSet<IWSData> IWSData { get; set; }
        public DbSet<MUEKSData> MUEKSData { get; set; }
        public DbSet<DOVData> DOVData { get; set; }
        public DbSet<DUSTData> DustData { get; set; }

        // Системные таблицы
        public DbSet<SensorResults> SensorResults { get; set; }
        public DbSet<SensorError> SensorError { get; set; }
        public DbSet<WorkerConfiguration> WorkerConfigurations { get; set; }

        // Авторизация
        public DbSet<User> Users { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }

        // Видеокамеры
        public DbSet<Camera> Cameras { get; set; }
        public DbSet<Snapshot> Snapshots { get; set; }

        // Веб-части дашборда
        public DbSet<WebPart> WebParts { get; set; }

        // Представления (VIEW)
        public DbSet<VwMueksDataFull> VwMueksDataFull { get; set; }
        public DbSet<VwIwsDataFull> VwIwsDataFull { get; set; }
        public DbSet<VwDspdDataFull> VwDspdDataFull { get; set; }
        public DbSet<VwDovDataFull> VwDovDataFull { get; set; }
        public DbSet<VwDustDataFull> VwDustDataFull { get; set; }
        public DbSet<VwSensorResultsFull> VwSensorResultsFull { get; set; }
        public DbSet<VwSensorErrorsFull> VwSensorErrorsFull { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Установка схемы по умолчанию
            modelBuilder.HasDefaultSchema("public");

            // Конфигурация представлений (без ключей)
            modelBuilder.Entity<VwMueksDataFull>()
                .HasNoKey()
                .ToView("vw_mueks_data_full");
            
            modelBuilder.Entity<VwIwsDataFull>()
                .HasNoKey()
                .ToView("vw_iws_data_full");
            
            modelBuilder.Entity<VwDspdDataFull>()
                .HasNoKey()
                .ToView("vw_dspd_data_full");
            
            modelBuilder.Entity<VwDovDataFull>()
                .HasNoKey()
                .ToView("vw_dov_data_full");
            
            modelBuilder.Entity<VwDustDataFull>()
                .HasNoKey()
                .ToView("vw_dust_data_full");
            
            modelBuilder.Entity<VwSensorResultsFull>()
                .HasNoKey()
                .ToView("vw_sensor_results_full");
            
            modelBuilder.Entity<VwSensorErrorsFull>()
                .HasNoKey()
                .ToView("vw_sensor_errors_full");
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            // Получаем информацию о пользователе
            string? userName = null;
            int? userId = null;
            
            // Если нет контекста пользователя (например, работает Worker), 
            // то userName будет null. В этом случае мы пропускаем аудит изменений,
            // так как аудит предназначен для отслеживания действий реальных пользователей.
            if (_httpContextAccessor?.HttpContext?.User.Identity?.IsAuthenticated == true)
            {
                userName = _httpContextAccessor.HttpContext.User.Identity.Name;
                var userIdClaim = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (userIdClaim != null && int.TryParse(userIdClaim, out var parsedUserId))
                    userId = parsedUserId;
            }

            // Если действие совершено не пользователем (например, системным процессом),
            // просто сохраняем изменения без создания логов аудита.
            if (string.IsNullOrEmpty(userName) || userName == "System")
            {
                return await base.SaveChangesAsync(cancellationToken);
            }

            // ВАЖНО: Сохраняем копии оригинальных значений ДО любых изменений
            var auditEntries = new List<(EntityEntry Entry, Dictionary<string, object?>? OriginalValues, Dictionary<string, object?>? CurrentValues)>();

            foreach (var entry in ChangeTracker.Entries())
            {
                if (entry.Entity is AuditLog) continue;

                if (entry.State == EntityState.Modified)
                {
                    var originalValues = new Dictionary<string, object?>();
                    var currentValues = new Dictionary<string, object?>();
                    
                    // Получаем значения из базы данных, если они не загружены или если мы хотим быть уверены
                    var databaseValues = await entry.GetDatabaseValuesAsync(cancellationToken);
                    
                    foreach (var property in entry.Properties)
                    {
                        if (property.Metadata.Name.Contains("Password", StringComparison.OrdinalIgnoreCase))
                            continue;

                        // Если есть значения из БД, берем их как оригинальные
                        var originalValue = databaseValues != null 
                            ? databaseValues[property.Metadata.Name] 
                            : property.OriginalValue;
                            
                        var currentValue = property.CurrentValue;

                        originalValues[property.Metadata.Name] = CloneValue(originalValue);
                        currentValues[property.Metadata.Name] = CloneValue(currentValue);

                        _logger?.LogDebug("Property {Prop}: Original={Original}, Current={Current}, IsModified={IsModified}", 
                            property.Metadata.Name, originalValue, currentValue, property.IsModified);
                    }

                    auditEntries.Add((entry, originalValues, currentValues));
                }
                else if (entry.State == EntityState.Added || entry.State == EntityState.Deleted)
                {
                    auditEntries.Add((entry, null, null));
                }
            }

            // Список для аудита изменений
            var changeAuditLogs = new List<AuditLog>();

            // Создаем логи на основе сохраненных копий
            foreach (var (entry, originalValues, currentValues) in auditEntries)
            {
                var log = CreateChangeAuditLog(entry, userName, userId, originalValues, currentValues);
                if (log != null)
                {
                    changeAuditLogs.Add(log);
                }
            }

            // Добавляем логи в контекст
            foreach (var log in changeAuditLogs)
            {
                await AuditLogs.AddAsync(log, cancellationToken);
            }

            // Сохраняем все изменения
            var result = await base.SaveChangesAsync(cancellationToken);
            
            _logger?.LogInformation("Saved {ChangeLogCount} change logs, total changes: {Result}", 
                changeAuditLogs.Count, result);
                
            return result;
        }

        private AuditLog? CreateChangeAuditLog(
            EntityEntry entry, 
            string? userName, 
            int? userId,
            Dictionary<string, object?>? originalValues,
            Dictionary<string, object?>? currentValues)
        {
            var entityType = entry.Entity.GetType().Name;
            var entityId = GetEntityId(entry);
            
            // Пропускаем логирование сущности аудита
            if (entityType.Contains("Log", StringComparison.OrdinalIgnoreCase) ||
                entityType.Contains("Audit", StringComparison.OrdinalIgnoreCase))
                return null;

            var changeType = entry.State.ToString();
            var log = new AuditLog
            {
                Type = AuditLogType.Change,
                EntityType = entityType,
                EntityId = entityId,
                ChangeType = changeType,
                UserName = userName ?? "System",
                UserId = userId,
                Timestamp = DateTime.UtcNow
            };

            try
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        log.NewValues = SerializeToJson(entry.CurrentValues);
                        _logger?.LogInformation("ADDED: {EntityType}", entityType);
                        break;

                    case EntityState.Deleted:
                        log.OriginalValues = SerializeToJson(entry.OriginalValues);
                        _logger?.LogInformation("DELETED: {EntityType} Id: {EntityId}", entityType, entityId);
                        break;

                    case EntityState.Modified:
                        if (originalValues == null || currentValues == null)
                        {
                            _logger?.LogWarning("Modified entity {EntityType} has null value dictionaries", entityType);
                            return null;
                        }

                        var changedProps = new List<object>();

                        foreach (var kvp in originalValues)
                        {
                            var propName = kvp.Key;
                            var originalValue = kvp.Value;
                            var currentValue = currentValues.ContainsKey(propName) ? currentValues[propName] : null;

                            // Проверяем, изменилось ли значение
                            if (!Equals(originalValue, currentValue))
                            {
                                changedProps.Add(new
                                {
                                    Property = propName,
                                    OldValue = originalValue?.ToString(),
                                    NewValue = currentValue?.ToString()
                                });
                                
                                _logger?.LogInformation("CHANGE DETECTED: {Property}: '{Original}' -> '{Current}'", 
                                    propName, originalValue, currentValue);
                            }
                        }

                        var jsonOptions = new JsonSerializerOptions 
                        { 
                            WriteIndented = false,
                            ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles
                        };
                        
                        log.OriginalValues = JsonSerializer.Serialize(originalValues, jsonOptions);
                        log.NewValues = JsonSerializer.Serialize(currentValues, jsonOptions);
                        
                        if (changedProps.Any())
                        {
                            log.ChangedProperties = JsonSerializer.Serialize(changedProps, jsonOptions);
                            _logger?.LogInformation("MODIFIED: {EntityType} Id: {EntityId}, Changed {Count} properties", 
                                entityType, entityId, changedProps.Count);
                        }
                        else
                        {
                            _logger?.LogInformation("MODIFIED: {EntityType} Id: {EntityId} - No actual changes detected", 
                                entityType, entityId);
                            // Если нет реальных изменений, не создаем лог
                            return null;
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error creating change log for {EntityType}", entityType);
                return null;
            }

            return log;
        }

        private object? CloneValue(object? value)
        {
            if (value == null) return null;
            
            var type = value.GetType();
            
            // Для примитивных типов и строк возвращаем как есть
            if (type.IsPrimitive || type == typeof(string) || type.IsEnum)
                return value;
            
            // Для DateTime создаем копию
            if (value is DateTime dateTime)
                return new DateTime(dateTime.Ticks, dateTime.Kind);
            
            // Для byte[] создаем копию
            if (value is byte[] bytes)
            {
                var copy = new byte[bytes.Length];
                Array.Copy(bytes, copy, bytes.Length);
                return copy;
            }
            
            // Для других ссылочных типов пробуем сериализовать/десериализовать
            try
            {
                var json = JsonSerializer.Serialize(value);
                return JsonSerializer.Deserialize(json, type);
            }
            catch
            {
                return value.ToString();
            }
        }

        private string SerializeToJson(PropertyValues propertyValues)
        {
            var dict = new Dictionary<string, object?>();
            foreach (var property in propertyValues.Properties)
            {
                if (property.Name.Contains("Password", StringComparison.OrdinalIgnoreCase))
                    continue;
                
                dict[property.Name] = CloneValue(propertyValues[property]);
            }
            
            var options = new JsonSerializerOptions 
            { 
                WriteIndented = false,
                ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles
            };
            
            return JsonSerializer.Serialize(dict, options);
        }

        private int? GetEntityId(EntityEntry entry)
        {
            try
            {
                var idProperty = entry.Properties.FirstOrDefault(p => p.Metadata.Name == "Id");
                if (idProperty != null)
                {
                    if (entry.State == EntityState.Added)
                        return null;
                    
                    return idProperty.CurrentValue as int?;
                }
            }
            catch { }
            return null;
        }
    }
}