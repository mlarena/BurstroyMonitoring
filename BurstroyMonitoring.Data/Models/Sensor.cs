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

        [Required(ErrorMessage = "Поле Тип датчика обязательно для заполнения")]
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

        [Required(ErrorMessage = "Поле Серийный номер обязательно для заполнения")]
        [StringLength(64)]
        [Column("SerialNumber")]
        public string SerialNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Поле Название обязательно для заполнения")]
        [StringLength(255)]
        [Column("EndPointsName")]
        public string EndPointsName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Поле URL API обязательно для заполнения")]
        [Column("Url")]
        public string Url { get; set; } = string.Empty;

        [Column("LastActivityUTC")]
        public DateTime? LastActivityUTC { get; set; }

        [Column("CreatedAt")]
        public DateTime? CreatedAt { get; set; }
        
        [Column("IsActive")]
        public bool IsActive { get; set; } = true;

        [Required(ErrorMessage = "Поле Пост мониторинга обязательно для заполнения")]
        [Column("MonitoringPostId")]
        public int MonitoringPostId { get; set; }

        [ForeignKey("MonitoringPostId")]
        public virtual MonitoringPost? MonitoringPost { get; set; }
    }
}