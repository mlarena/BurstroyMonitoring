using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BurstroyMonitoring.Data.Models;

/// <summary>
/// Модель данных датчика PUID
/// </summary>
[Table("PuidData", Schema = "public")]
public class PuidData
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    public int PuidId { get; set; }
    
    [ForeignKey(nameof(PuidId))]
    public virtual Puid? Puid { get; set; }

    public Guid MessageId { get; set; }
    public Guid SensorId { get; set; }
    public string? SensorName { get; set; }
    public int Direction { get; set; }
    public int Lane { get; set; }
    public int Volume { get; set; }
    public int Class0 { get; set; }
    public int Class1 { get; set; }
    public int Class2 { get; set; }
    public int Class3 { get; set; }
    public int Class4 { get; set; }
    public int Class5 { get; set; }
    public double GapAvg { get; set; }
    public double GapSum { get; set; }
    public double SpeedAvg { get; set; }
    public double HeadwayAvg { get; set; }
    public double HeadwaySum { get; set; }
    public double Speed85Avg { get; set; }
    public string? OccupancyPer { get; set; }
    public double OccupancyPrc { get; set; }
    public double OccupancySum { get; set; }
    public DateTime RangeStart { get; set; }
    public DateTime RangeEnd { get; set; }
    public int RangeValue { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
