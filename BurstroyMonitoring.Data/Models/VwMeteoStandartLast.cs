using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BurstroyMonitoring.Data.Models
{
    /// <summary>
    /// View Model для представления public.vw_meteo_standart_last
    /// </summary>
    [Table("vw_meteo_standart_last", Schema = "public")]
    public class VwMeteoStandartLast
    {
        [Key]
        [Column("PollingSessionId")]
        public Guid PollingSessionId { get; set; }

        [Column("MonitoringPostId")]        public int? MonitoringPostId { get; set; }

        [Column("StartedAt")]
        public DateTime? StartedAt { get; set; }

        [Column("CompletedAt")]
        public DateTime? CompletedAt { get; set; }

        [Column("Status")]
        public string? Status { get; set; }

        // DOVData
        [Column("VisibleRange")]
        public decimal? VisibleRange { get; set; }

        // IWSData
        [Column("EnvTemperature")]
        public decimal? EnvTemperature { get; set; }

        [Column("Humidity")]
        public decimal? Humidity { get; set; }

        [Column("DewPoint")]
        public decimal? DewPoint { get; set; }

        [Column("PressureHPa")]
        public decimal? PressureHPa { get; set; }

        [Column("PressureQNHHPa")]
        public decimal? PressureQNHHPa { get; set; }

        [Column("PressureMmHg")]
        public decimal? PressureMmHg { get; set; }

        [Column("WindSpeed")]
        public decimal? WindSpeed { get; set; }

        [Column("WindDirection")]
        public decimal? WindDirection { get; set; }

        [Column("WindVSound")]
        public decimal? WindVSound { get; set; }

        [Column("PrecipitationType")]
        public int? PrecipitationType { get; set; }

        [Column("PrecipitationIntensity")]
        public decimal? PrecipitationIntensity { get; set; }

        [Column("PrecipitationQuantity")]
        public decimal? PrecipitationQuantity { get; set; }

        [Column("PrecipitationElapsed")]
        public int? PrecipitationElapsed { get; set; }

        [Column("PrecipitationPeriod")]
        public int? PrecipitationPeriod { get; set; }

        // DSPDData
        [Column("Grip")]
        public decimal? Grip { get; set; }

        [Column("Shake")]
        public decimal? Shake { get; set; }

        [Column("UPower")]
        public decimal? UPower { get; set; }

        [Column("TemperatureCase")]
        public decimal? TemperatureCase { get; set; }

        [Column("TemperatureRoad")]
        public decimal? TemperatureRoad { get; set; }

        [Column("HeightH2O")]
        public decimal? HeightH2O { get; set; }

        [Column("HeightIce")]
        public decimal? HeightIce { get; set; }

        [Column("HeightSnow")]
        public decimal? HeightSnow { get; set; }

        [Column("PercentICE")]
        public decimal? PercentICE { get; set; }

        [Column("PercentPGM")]
        public decimal? PercentPGM { get; set; }

        [Column("RoadStatus")]
        public int? RoadStatus { get; set; }

        [Column("AngleToRoad")]
        public decimal? AngleToRoad { get; set; }

        [Column("TemperatureFreezePGM")]
        public decimal? TemperatureFreezePGM { get; set; }
    }
}
