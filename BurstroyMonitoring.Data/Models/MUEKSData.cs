using System.ComponentModel.DataAnnotations.Schema;

namespace BurstroyMonitoring.Data.Models;

/// <summary>
/// Модель данных датчика MUEKS
/// </summary>
[Table("MUEKSData", Schema = "public")]
public class MUEKSData
{
    public int Id { get; set; }
    public int SensorId { get; set; }
    public DateTime ReceivedAt { get; set; }
    public DateTime DataTimestamp { get; set; }
    
    public decimal? TemperatureBox { get; set; }
    public decimal? UPowerIn12B { get; set; }
    public decimal? UOut12B { get; set; }
    public decimal? IOut12B { get; set; }
    public decimal? IOut48B { get; set; }
    public decimal? UAkb { get; set; }
    public decimal? IAkb { get; set; }
    public int? Sens220B { get; set; }
    public decimal? WhAkb { get; set; }
    public decimal? VisibleRange { get; set; }
    public int? DoorStatus { get; set; }
    public string? TdsH { get; set; }
    public string? TdsTds { get; set; }
    public string? TkosaT1 { get; set; }
    public string? TkosaT3 { get; set; }
    
    // Навигационное свойство
    public Sensor? Sensor { get; set; }
}