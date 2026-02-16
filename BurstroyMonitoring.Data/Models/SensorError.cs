using System.Text.Json;
using System.ComponentModel.DataAnnotations.Schema;

namespace BurstroyMonitoring.Data.Models;

/// <summary>
/// Модель ошибки датчика
/// </summary>
[Table("SensorError", Schema = "public")]
public class SensorError
{
    public int Id { get; set; }
    public int SensorId { get; set; }
    public string ErrorLevel { get; set; } = string.Empty;
    public string ErrorMessage { get; set; } = string.Empty;
    public string? StackTrace { get; set; }
    public string? ErrorSource { get; set; }
    public string? ExceptionType { get; set; }
    public DateTime CreatedAt { get; set; }
    public JsonDocument? AdditionalData { get; set; }

    // Навигационное свойство
    public Sensor? Sensor { get; set; }
}