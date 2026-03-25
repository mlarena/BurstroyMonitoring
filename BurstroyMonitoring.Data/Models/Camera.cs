using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BurstroyMonitoring.Data.Models;

[Table("Cameras", Schema = "public")]
public class Camera
{
    [Key]
    public int Id { get; set; }

    [Required]
    [Display(Name = "Название камеры")]
    public string Name { get; set; } = string.Empty;

    [Display(Name = "Место установки")]
    public string? InstallationLocation { get; set; }

    [Display(Name = "Запрос API")]
    public string? ApiRequest { get; set; }

    [Required]
    [Display(Name = "Ссылка на видеопоток")]
    public string RtspUrl { get; set; } = string.Empty;

    [Display(Name = "Серийный номер")]
    public string? SerialNumber { get; set; }

    [Display(Name = "Логин")]
    public string? Username { get; set; }

    [Display(Name = "Пароль")]
    public string? Password { get; set; }

    [Display(Name = "URL-адрес")]
    public string? WebUrl { get; set; }

    [Display(Name = "Пикет")]
    public string? Picket { get; set; }

    [Display(Name = "Id комплекса в системе FDA")]
    public string? FdaId { get; set; }

    [Display(Name = "Интервал опроса, сек")]
    public int PollingInterval { get; set; } = 10;

    [Display(Name = "Пост мониторинга")]
    public int? MonitoringPostId { get; set; }

    [ForeignKey("MonitoringPostId")]
    public virtual MonitoringPost? MonitoringPost { get; set; }

    [Display(Name = "Поддержка PTZ")]
    public bool IsPtzSupported { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc);
    
    public virtual ICollection<Snapshot> Snapshots { get; set; } = new List<Snapshot>();
}
