using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace BurstroyMonitoring.Data.Models
{
    /// <summary>
    /// View Model для vw_mueks_data_full
    /// Обновлено: добавлены sensor_id, post_id
    /// </summary>
    [Table("vw_mueks_data_full", Schema = "public")]
    public class VwMueksDataFull
    {
        // ========== MUEKSData поля ==========
        [Column("mueks_data_id")]
        public int MueksDataId { get; set; }

        [Column("received_at")]
        public DateTime? ReceivedAt { get; set; }

        [Column("data_timestamp")]
        public DateTime? DataTimestamp { get; set; }

        [Column("temperature_box")]
        public decimal? TemperatureBox { get; set; }

        [Column("voltage_power_in_12b")]
        public decimal? VoltagePowerIn12B { get; set; }

        [Column("voltage_out_12b")]
        public decimal? VoltageOut12B { get; set; }

        [Column("current_out_12b")]
        public decimal? CurrentOut12B { get; set; }

        [Column("current_out_48b")]
        public decimal? CurrentOut48B { get; set; }

        [Column("voltage_akb")]
        public decimal? VoltageAkb { get; set; }

        [Column("current_akb")]
        public decimal? CurrentAkb { get; set; }

        [Column("sensor_220b")]
        public int? Sensor220B { get; set; }

        [Column("watt_hours_akb")]
        public decimal? WattHoursAkb { get; set; }

        [Column("visible_range")]
        public decimal? VisibleRange { get; set; }

        [Column("door_status")]
        public int? DoorStatus { get; set; }

        [Column("tds_h")]
        public string? TdsH { get; set; }

        [Column("tds_tds")]
        public string? TdsTds { get; set; }

        [Column("tkosa_t1")]
        public string? TkosaT1 { get; set; }

        [Column("tkosa_t3")]
        public string? TkosaT3 { get; set; }

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