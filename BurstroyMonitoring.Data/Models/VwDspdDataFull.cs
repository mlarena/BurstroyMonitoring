using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace BurstroyMonitoring.Data.Models
{
    /// <summary>
    /// View Model для vw_dspd_data_full
    /// Обновлено: добавлены sensor_id, post_id
    /// </summary>
    [Table("vw_dspd_data_full", Schema = "public")]
    public class VwDspdDataFull
    {
        // ========== DSPDData поля ==========
        [Column("dspd_data_id")]
        public int DspdDataId { get; set; }

        [Column("received_at")]
        public DateTime? ReceivedAt { get; set; }

        [Column("data_timestamp")]
        public DateTime? DataTimestamp { get; set; }

        [Column("grip_coefficient")]
        public decimal? GripCoefficient { get; set; }

        [Column("shake_level")]
        public decimal? ShakeLevel { get; set; }

        [Column("voltage_power")]
        public decimal? VoltagePower { get; set; }

        [Column("case_temperature")]
        public decimal? CaseTemperature { get; set; }

        [Column("road_temperature")]
        public decimal? RoadTemperature { get; set; }

        [Column("water_height")]
        public decimal? WaterHeight { get; set; }

        [Column("ice_height")]
        public decimal? IceHeight { get; set; }

        [Column("snow_height")]
        public decimal? SnowHeight { get; set; }

        [Column("ice_percentage")]
        public decimal? IcePercentage { get; set; }

        [Column("pgm_percentage")]
        public decimal? PgmPercentage { get; set; }

        [Column("road_status_code")]
        public int? RoadStatusCode { get; set; }

        [Column("road_angle")]
        public decimal? RoadAngle { get; set; }

        [Column("freeze_temperature")]
        public decimal? FreezeTemperature { get; set; }

        [Column("calibration_needed")]
        public int? CalibrationNeeded { get; set; }

        [Column("gps_latitude")]
        public decimal? GpsLatitude { get; set; }

        [Column("gps_longitude")]
        public decimal? GpsLongitude { get; set; }

        [Column("gps_valid")]
        public bool? GpsValid { get; set; }

        [Column("distance_to_surface")]
        public double? DistanceToSurface { get; set; }

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