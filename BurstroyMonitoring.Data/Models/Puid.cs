using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BurstroyMonitoring.Data.Models;

/// <summary>
/// Модель устройства PUID
/// </summary>
[Table("Puid", Schema = "public")]
public class Puid
{
    [Key]
    public int Id { get; set; }
    
    [Required(ErrorMessage = "Тип сенсора обязателен")]
    [MaxLength(64)]
    public string SensorType { get; set; } = "PUID";
    
    [Required(ErrorMessage = "Пост мониторинга обязателен")]
    public int? MonitoringPostId { get; set; }
    
    public double? Longitude { get; set; }
    
    public double? Latitude { get; set; }
    
    [Required(ErrorMessage = "Серийный номер обязателен")]
    [MaxLength(64)]
    public string SerialNumber { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Название конечной точки обязательно")]
    [MaxLength(255)]
    public string EndPointsName { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Интервал опроса обязателен")]
    [Range(10, 3600, ErrorMessage = "Интервал должен быть от 10 до 3600 секунд")]
    public int IntervalSeconds { get; set; } = 60;
    
    [Required(ErrorMessage = "URL API обязателен")]
    [Url(ErrorMessage = "Некорректный формат URL")]
    public string Url { get; set; } = string.Empty;    
    public DateTime? LastActivityUTC { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public bool IsActive { get; set; } = true;

    // Навигационные свойства
    [ForeignKey(nameof(MonitoringPostId))]
    public virtual MonitoringPost? MonitoringPost { get; set; }
    
    public virtual ICollection<PuidData> Data { get; set; } = new List<PuidData>();
}
