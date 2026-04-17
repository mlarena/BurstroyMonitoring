using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BurstroyMonitoring.Data.Models;

/// <summary>
/// Результаты опроса датчика PUID
/// </summary>
[Table("PuidResults", Schema = "public")]
public class PuidResults
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    public int PuidId { get; set; }

    [ForeignKey(nameof(PuidId))]
    public virtual Puid? Puid { get; set; }

    [Required]
    public DateTime CheckedAt { get; set; }

    [Required]
    public int StatusCode { get; set; }

    [Column(TypeName = "jsonb")]
    public string? ResponseBody { get; set; }

    [Required]
    public long ResponseTimeMs { get; set; }

    [Required]
    public bool IsSuccess { get; set; }
}
