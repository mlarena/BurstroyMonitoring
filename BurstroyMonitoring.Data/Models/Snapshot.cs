using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BurstroyMonitoring.Data.Models;

[Table("Snapshots", Schema = "public")]
public class Snapshot
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int CameraId { get; set; }

    [ForeignKey("CameraId")]
    public virtual Camera? Camera { get; set; }

    [Required]
    public string FilePath { get; set; } = string.Empty;

    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc);
}
