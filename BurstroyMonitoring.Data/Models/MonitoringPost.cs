using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BurstroyMonitoring.Data.Models
{
    [Table("MonitoringPost", Schema = "public")]
    public class MonitoringPost
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("Id")]
        public int Id { get; set; }

        [Required]
        [StringLength(255)]
        [Column("Name")]
        public string Name { get; set; } = string.Empty;

        [Column("Description")]
        public string? Description { get; set; }

        [Column("Longitude")]
        [Range(-180.0, 180.0, ErrorMessage = "Долгота должна быть от -180 до 180")]
        public double? Longitude { get; set; }

        [Column("Latitude")]
        [Range(-90.0, 90.0, ErrorMessage = "Широта должна быть от -90 до 90")]
        public double? Latitude { get; set; }

        [Column("IsMobile")]
        public bool IsMobile { get; set; } = false;

        [Column("IsActive")]
        public bool IsActive { get; set; } = true;
        
        [Column("CreatedAt")]
        public DateTime? CreatedAt { get; set; }

        [Column("UpdatedAt")]
        public DateTime? UpdatedAt { get; set; }

        // Навигационное свойство
        public virtual ICollection<Sensor> Sensors { get; set; } = new List<Sensor>();
    }
}