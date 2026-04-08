using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BurstroyMonitoring.Data.Models
{
    [Table("SensorType", Schema = "public")]
    public class SensorType
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("Id")]
        public int Id { get; set; }

        [Required(ErrorMessage = "Поле Название типа обязательно для заполнения")]
        [StringLength(20)]
        [Column("SensorTypeName")]
        public string SensorTypeName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Поле Описание обязательно для заполнения")]
        [Column("Description")]
        public string Description { get; set; } = string.Empty;
        
        [Column("CreatedAt")]
        public DateTime? CreatedAt { get; set; }

        // Навигационное свойство
        public virtual ICollection<Sensor> Sensors { get; set; } = new List<Sensor>();
    }
}