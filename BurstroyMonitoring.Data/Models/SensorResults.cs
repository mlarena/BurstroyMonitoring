using System.Text.Json;
using System.ComponentModel.DataAnnotations.Schema;

namespace BurstroyMonitoring.Data.Models;

/// <summary>
/// Модель результата опроса датчика
/// </summary>
[Table("SensorResults", Schema = "public")]
public class SensorResults
{
    public int Id { get; set; }
    public int SensorId { get; set; }
    public DateTime CheckedAt { get; set; }
    public int StatusCode { get; set; }
    public JsonDocument? ResponseBody { get; set; }
    public long ResponseTimeMs { get; set; }
    public bool IsSuccess { get; set; }

    // Навигационное свойство
    public Sensor? Sensor { get; set; }
}