using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BurstroyMonitoring.Data.Models
{
    [Table("Sensor", Schema = "public")]
    public class Sensor
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("Id")]
        public int Id { get; set; }

        [Required]
        [Column("SensorTypeId")]
        public int SensorTypeId { get; set; }

        [ForeignKey("SensorTypeId")]
        public virtual SensorType? SensorType { get; set; }

        [Column("Longitude")]
        [Range(-180.0, 180.0, ErrorMessage = "Долгота должна быть от -180 до 180")]
        public double? Longitude { get; set; }

        [Column("Latitude")]
        [Range(-90.0, 90.0, ErrorMessage = "Широта должна быть от -90 до 90")]
        public double? Latitude { get; set; }

        [Required]
        [StringLength(64)]
        [Column("SerialNumber")]
        public string SerialNumber { get; set; } = string.Empty;

        [Required]
        [StringLength(255)]
        [Column("EndPointsName")]
        public string EndPointsName { get; set; } = string.Empty;

        [Required]
        [Column("Url")]
        public string Url { get; set; } = string.Empty;

        [Column("CheckIntervalSeconds")]
        public int CheckIntervalSeconds { get; set; } = 300;

        [Column("LastActivityUTC")]
        public DateTime? LastActivityUTC { get; set; }

        [Column("CreatedAt")]
        public DateTime? CreatedAt { get; set; }
        
        [Column("IsActive")]
        public bool IsActive { get; set; } = true;

        [Column("MonitoringPostId")]
        public int? MonitoringPostId { get; set; }

        [ForeignKey("MonitoringPostId")]
        public virtual MonitoringPost? MonitoringPost { get; set; }
    }
}