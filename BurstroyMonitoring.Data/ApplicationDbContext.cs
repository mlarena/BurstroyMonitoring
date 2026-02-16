using Microsoft.EntityFrameworkCore;
using BurstroyMonitoring.Data.Models;

namespace BurstroyMonitoring.Data
{
    public class ApplicationDbContext : DbContext
    {
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
            var entries = ChangeTracker.Entries()
                .Where(e => e.Entity is SensorError && e.State == EntityState.Added);

            foreach (var entry in entries)
            {
                if (entry.Entity is SensorError error)
                {
                    error.CreatedAt = DateTime.UtcNow;
                }
            }

            return await base.SaveChangesAsync(cancellationToken);
        }
    }
}