using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace BurstroyMonitoring.Data.Models
{
    /// <summary>
    /// View Model для vw_iws_data_full
    /// Обновлено: добавлены sensor_id, post_id, co2_level
    /// </summary>
    [Table("vw_iws_data_full", Schema = "public")]
    public class VwIwsDataFull
    {
        // ========== IWSData поля ==========
        [Column("iws_data_id")]
        public int IwsDataId { get; set; }

        [Column("received_at")]
        public DateTime? ReceivedAt { get; set; }

        [Column("data_timestamp")]
        public DateTime? DataTimestamp { get; set; }

        [Column("environment_temperature")]
        public decimal? EnvironmentTemperature { get; set; }

        [Column("humidity_percentage")]
        public decimal? HumidityPercentage { get; set; }

        [Column("dew_point")]
        public decimal? DewPoint { get; set; }

        [Column("pressure_hpa")]
        public decimal? PressureHpa { get; set; }

        [Column("pressure_qnh_hpa")]
        public decimal? PressureQnhHpa { get; set; }

        [Column("pressure_mmhg")]
        public decimal? PressureMmhg { get; set; }

        [Column("wind_speed")]
        public decimal? WindSpeed { get; set; }

        [Column("wind_direction")]
        public decimal? WindDirection { get; set; }

        [Column("wind_vs_sound")]
        public decimal? WindVsSound { get; set; }

        [Column("precipitation_type")]
        public int? PrecipitationType { get; set; }

        [Column("precipitation_intensity")]
        public decimal? PrecipitationIntensity { get; set; }

        [Column("precipitation_quantity")]
        public decimal? PrecipitationQuantity { get; set; }

        [Column("precipitation_elapsed")]
        public int? PrecipitationElapsed { get; set; }

        [Column("precipitation_period")]
        public int? PrecipitationPeriod { get; set; }

        [Column("co2_level")]
        public decimal? CO2Level { get; set; }

        [Column("supply_voltage")]
        public decimal? SupplyVoltage { get; set; }

        [Column("iws_latitude")]
        public decimal? IwsLatitude { get; set; }

        [Column("iws_longitude")]
        public decimal? IwsLongitude { get; set; }

        [Column("altitude")]
        public decimal? Altitude { get; set; }

        [Column("ksp_value")]
        public int? KspValue { get; set; }

        [Column("gps_speed")]
        public decimal? GpsSpeed { get; set; }

        [Column("acceleration_std_dev")]
        public decimal? AccelerationStdDev { get; set; }

        [Column("roll_angle")]
        public decimal? RollAngle { get; set; }

        [Column("pitch_angle")]
        public decimal? PitchAngle { get; set; }

        [Column("status_ok")]
        public int? StatusOk { get; set; }

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