using System.ComponentModel.DataAnnotations.Schema;

namespace BurstroyMonitoring.Data.Models;

/// <summary>
/// Модель данных датчика DSPD
/// </summary>
[Table("DSPDData", Schema = "public")]
public class DSPDData
{
    public int Id { get; set; }
    public int SensorId { get; set; }
    public DateTime ReceivedAt { get; set; }
    public DateTime DataTimestamp { get; set; }
    
    public decimal? Grip { get; set; }
    public decimal? Shake { get; set; }
    public decimal? UPower { get; set; }
    public decimal? TemperatureCase { get; set; }
    public decimal? TemperatureRoad { get; set; }
    public decimal? HeightH2O { get; set; }
    public decimal? HeightIce { get; set; }
    public decimal? HeightSnow { get; set; }
    public decimal? PercentICE { get; set; }
    public decimal? PercentPGM { get; set; }
    public int? RoadStatus { get; set; }
    public decimal? AngleToRoad { get; set; }
    public decimal? TemperatureFreezePGM { get; set; }
    public int? NeedCalibration { get; set; }
    public decimal? GPSLatitude { get; set; }
    public decimal? GPSLongitude { get; set; }
    public bool IsGpsValid { get; set; } = true;
    public decimal? DistanceToSurface { get; set; }
    // Навигационное свойство
    public Sensor? Sensor { get; set; }
}