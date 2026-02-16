using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace BurstroyMonitoring.Data.Models
{
    /// <summary>
    /// View Model для vw_dust_data_full
    /// Исправлено: dov_data_id -> dust_data_id
    /// Обновлено: добавлены sensor_id, post_id
    /// </summary>
    [Table("vw_dust_data_full", Schema = "public")]
    public class VwDustDataFull
    {
        // ========== DUSTData поля ==========
        [Column("dust_data_id")]  // было "dust_data_id", это правильно
        public int DustDataId { get; set; }

        [Column("received_at")]   // было "ReceivedAt" - ИСПРАВЛЕНО
        public DateTime? ReceivedAt { get; set; }

        [Column("data_timestamp")] // было "DataTimestamp" - ИСПРАВЛЕНО
        public DateTime? DataTimestamp { get; set; }
        
        [Column("pm10act")]        // было "PM10Act" - ИСПРАВЛЕНО
        public decimal? PM10Act { get; set; }
        
        [Column("pm25act")]        // было "PM25Act" - ИСПРАВЛЕНО
        public decimal? PM25Act { get; set; }
        
        [Column("pm1act")]         // было "PM1Act" - ИСПРАВЛЕНО
        public decimal? PM1Act { get; set; }
        
        [Column("pm10awg")]        // было "PM10AWG" - ИСПРАВЛЕНО
        public decimal? PM10AWG { get; set; }
        
        [Column("pm25awg")]        // было "PM25AWG" - ИСПРАВЛЕНО
        public decimal? PM25AWG { get; set; }
        
        [Column("pm1awg")]         // было "PM1AWG" - ИСПРАВЛЕНО
        public decimal? PM1AWG { get; set; }
        
        [Column("flowprobe")]      // было "FlowProbe" - ИСПРАВЛЕНО!
        public decimal? FlowProbe { get; set; }
        
        [Column("temperatureprobe")] // было "TemperatureProbe" - ИСПРАВЛЕНО
        public decimal? TemperatureProbe { get; set; }
        
        [Column("humidityprobe")]    // было "HumidityProbe" - ИСПРАВЛЕНО
        public decimal? HumidityProbe { get; set; }
        
        [Column("laserstatus")]      // было "LaserStatus" - ИСПРАВЛЕНО
        public int? LaserStatus { get; set; }
        
        [Column("supplyvoltage")]    // было "SupplyVoltage" - ИСПРАВЛЕНО
        public decimal? SupplyVoltage { get; set; }
        
        // ========== Sensor поля ==========
        [Column("sensor_id")]
        public int? SensorId { get; set; }

        [Column("sensor_longitude")]
        public double? SensorLongitude { get; set; }

        [Column("sensor_latitude")]
        public double? SensorLatitude { get; set; }

        [Column("serial_number")]
        public string? SerialNumber { get; set; }

        [Column("endpoint_name")]
        public string? EndpointName { get; set; }

        [Column("sensor_url")]
        public string? SensorUrl { get; set; }

        [Column("check_interval_seconds")]
        public int? CheckIntervalSeconds { get; set; }

        [Column("last_activity_utc")]
        public DateTime? LastActivityUtc { get; set; }

        [Column("sensor_is_active")]
        public bool? SensorIsActive { get; set; }

        // ========== SensorType поля ==========
        [Column("sensor_type_name")]
        public string? SensorTypeName { get; set; }

        [Column("sensor_type_description")]
        public string? SensorTypeDescription { get; set; }

        // ========== MonitoringPost поля ==========
        [Column("post_id")]
        public int? PostId { get; set; }

        [Column("post_name")]
        public string? PostName { get; set; }

        [Column("post_description")]
        public string? PostDescription { get; set; }

        [Column("post_is_mobile")]
        public bool? PostIsMobile { get; set; }

        [Column("post_is_active")]
        public bool? PostIsActive { get; set; }
    }
}