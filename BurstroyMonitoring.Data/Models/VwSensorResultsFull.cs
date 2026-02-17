using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace BurstroyMonitoring.Data.Models
{
    /// <summary>
    /// View Model для vw_sensor_results_full
    /// Обновлено: добавлены sensor_id, post_id, sensor_type_id
    /// </summary>
    [Table("vw_sensor_results_full", Schema = "public")]
    public class VwSensorResultsFull
    {
        // ========== SensorResults поля ==========
        [Column("result_id")]
        public int ResultId { get; set; }

        [Column("checked_at")]
        public DateTime? CheckedAt { get; set; }

        [Column("status_code")]
        public int? StatusCode { get; set; }

        [Column("response_body")]
        public string? ResponseBody { get; set; }

        [Column("response_time_ms")]
        public int? ResponseTimeMs { get; set; }

        [Column("is_success")]
        public bool? IsSuccess { get; set; }

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
        [Column("sensor_type_id")]  // НОВОЕ ПОЛЕ
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