using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace BurstroyMonitoring.Data.Models
{
    /// <summary>
    /// View Model для vw_dov_data_full
    /// Обновлено: добавлены sensor_id, post_id
    /// </summary>
    [Table("vw_dov_data_full", Schema = "public")]
    public class VwDovDataFull
    {
        // ========== DOVData поля ==========
        [Column("dov_data_id")]
        public int DovDataId { get; set; }

        [Column("received_at")]
        public DateTime? ReceivedAt { get; set; }

        [Column("data_timestamp")]
        public DateTime? DataTimestamp { get; set; }

        [Column("visible_range")]
        public decimal? VisibleRange { get; set; }

        [Column("bright_flag")]
        public int? BrightFlag { get; set; }

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
        [Column("sensor_type_id")]  // ДОБАВИТЬ!
        public int? SensorTypeId { get; set; }

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