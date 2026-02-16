using System.ComponentModel.DataAnnotations.Schema;

namespace BurstroyMonitoring.Data.Models;

/// <summary>
/// Модель данных датчика DUST
/// </summary>
[Table("DustData", Schema = "public")]
public class DUSTData
{
    public int Id { get; set; }
    public int SensorId { get; set; }
    public DateTime ReceivedAt { get; set; }
    public DateTime DataTimestamp { get; set; }
    
    // Значения размер частиц микрограмм на куб метр
    public decimal? PM10Act { get; set; }
    public decimal? PM25Act { get; set; }
    public decimal? PM1Act { get; set; }
    
    // Средние значения частиц микрограмм на куб метр
    public decimal? PM10AWG { get; set; }
    public decimal? PM25AWG { get; set; }
    public decimal? PM1AWG { get; set; }
    
    // Данные датчиков
    public decimal? FlowProbe { get; set; }
    public decimal? TemperatureProbe { get; set; }
    public decimal? HumidityProbe { get; set; }
    
    // Статус и питание
    public int? LaserStatus { get; set; }
    public decimal? SupplyVoltage { get; set; }
    
    // Навигационное свойство
    public Sensor? Sensor { get; set; }
}