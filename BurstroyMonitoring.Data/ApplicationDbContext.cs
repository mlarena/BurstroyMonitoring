using Microsoft.EntityFrameworkCore;
using BurstroyMonitoring.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking; 
using System.Text.Json;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore.Metadata;

namespace BurstroyMonitoring.Data
{
    public class ApplicationDbContext : DbContext
    {
        private readonly IHttpContextAccessor? _httpContextAccessor;

       public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, IHttpContextAccessor httpContextAccessor)
            : base(options)
        {
            _httpContextAccessor = httpContextAccessor;
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
            // Автоматическая установка времени для определенных полей
            // var entries = ChangeTracker.Entries()
            //     .Where(e => e.Entity is SensorError && e.State == EntityState.Added);

            // foreach (var entry in entries)
            // {
            //     if (entry.Entity is SensorError error)
            //     {
            //         error.CreatedAt = DateTime.UtcNow;
            //     }
            // }

            // Получаем информацию о пользователе
            string? userName = null;
            int? userId = null;
            
            if (_httpContextAccessor?.HttpContext?.User.Identity?.IsAuthenticated == true)
            {
                userName = _httpContextAccessor.HttpContext.User.Identity.Name;
                var userIdClaim = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (userIdClaim != null && int.TryParse(userIdClaim, out var parsedUserId))
                    userId = parsedUserId;
            }

            // Отслеживаем изменения до сохранения
            var entries = ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Added || 
                           e.State == EntityState.Modified || 
                           e.State == EntityState.Deleted)
                .Where(e => e.Entity is not AuditLog) // не логируем аудит-логи
                .ToList();

            // Список для аудита изменений
            var changeAuditLogs = new List<AuditLog>();

            // Логируем изменения
            foreach (var entry in entries)
            {
                var log = CreateChangeAuditLog(entry, userName, userId);
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

            return await base.SaveChangesAsync(cancellationToken);
        }

        private AuditLog? CreateChangeAuditLog(EntityEntry entry, string? userName, int? userId)
        {
            var entityType = entry.Entity.GetType().Name;
            var entityId = GetEntityId(entry);
            
            // Пропускаем логирование сущности аудита (и любых сущностей логов, если останутся)
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
                        log.NewValues = JsonSerializer.Serialize(entry.CurrentValues.ToObject());
                        break;

                    case EntityState.Deleted:
                        log.OriginalValues = JsonSerializer.Serialize(entry.OriginalValues.ToObject());
                        break;

                    case EntityState.Modified:
                        var original = entry.OriginalValues.ToObject();
                        var current = entry.CurrentValues.ToObject();
                        
                        log.OriginalValues = JsonSerializer.Serialize(original);
                        log.NewValues = JsonSerializer.Serialize(current);
                        
                        // Определяем какие свойства изменились
                        var changedProps = entry.Properties
                            .Where(p => p.IsModified && !p.Metadata.Name.Contains("Password", StringComparison.OrdinalIgnoreCase))
                            .Select(p => new
                            {
                                Property = p.Metadata.Name,
                                OldValue = p.OriginalValue?.ToString(),
                                NewValue = p.CurrentValue?.ToString()
                            })
                            .ToList();
                        
                        if (changedProps.Any())
                        {
                            log.ChangedProperties = JsonSerializer.Serialize(changedProps);
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error creating change log: {ex.Message}");
                return null;
            }

            return log;
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
            catch
            {
                // Игнорируем ошибки
            }
            return null;
        }
    }
}