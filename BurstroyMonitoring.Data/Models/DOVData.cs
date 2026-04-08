using System.ComponentModel.DataAnnotations.Schema;

namespace BurstroyMonitoring.Data.Models;

/// <summary>
/// Модель данных датчика DOV
/// </summary>
[Table("DOVData", Schema = "public")]
public class DOVData
{
    public int Id { get; set; }
    public int SensorId { get; set; }
    public DateTime ReceivedAt { get; set; }
    public DateTime DataTimestamp { get; set; }
    
    public decimal? VisibleRange { get; set; }
    public int? BrightFlag { get; set; }
    
    [Column("PollingSessionId")]
    public Guid? PollingSessionId { get; set; }
    
    [Column("MonitoringPostId")]
    public int? MonitoringPostId { get; set; }

    // Навигационные свойства
    public Sensor? Sensor { get; set; }
    
    [ForeignKey("PollingSessionId")]
    public virtual PollingSession? PollingSession { get; set; }
    
    [ForeignKey("MonitoringPostId")]
    public virtual MonitoringPost? MonitoringPost { get; set; }
}