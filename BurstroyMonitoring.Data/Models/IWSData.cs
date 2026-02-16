using System.ComponentModel.DataAnnotations.Schema;

namespace BurstroyMonitoring.Data.Models;

/// <summary>
/// Модель данных датчика IWS
/// </summary>
[Table("IWSData", Schema = "public")]
public class IWSData
{
    public int Id { get; set; }
    public int SensorId { get; set; }
    public DateTime ReceivedAt { get; set; }
    public DateTime DataTimestamp { get; set; }
    
    public decimal? EnvTemperature { get; set; }
    public decimal? Humidity { get; set; }
    public decimal? DewPoint { get; set; }
    public decimal? PressureHPa { get; set; }
    public decimal? PressureQNHHPa { get; set; }
    public decimal? PressureMmHg { get; set; }
    public decimal? WindSpeed { get; set; }
    public decimal? WindDirection { get; set; }
    public decimal? WindVSound { get; set; }
    public int? PrecipitationType { get; set; }
    public decimal? PrecipitationIntensity { get; set; }
    public decimal? PrecipitationQuantity { get; set; }
    public int? PrecipitationElapsed { get; set; }
    public int? PrecipitationPeriod { get; set; }
    public decimal? CO2Level { get; set; } 
    public decimal? SupplyVoltage { get; set; }
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    public decimal? Altitude { get; set; }
    public int? KSP { get; set; }
    public decimal? GPSSpeed { get; set; }
    public decimal? AccelerationStDev { get; set; }
    public decimal? Roll { get; set; }
    public decimal? Pitch { get; set; }
    public int? WeAreFine { get; set; }
    
    // Навигационное свойство
    public Sensor? Sensor { get; set; }
}