using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BurstroyMonitoring.Data.Models
{
    [Table("WorkerConfiguration", Schema = "public")]
    public class WorkerConfiguration
    {
        [Key]
        [Column("Id")]
        public int Id { get; set; }

        [Required]
        [Column("Key")]
        [StringLength(100)]
        public string Key { get; set; } = string.Empty;

        [Required]
        [Column("Value")]
        public string Value { get; set; } = string.Empty;

        [Required]
        [Column("DataType")]
        [StringLength(50)]
        public string DataType { get; set; } = string.Empty;

        [Column("Description")]
        public string? Description { get; set; }

        [Column("LastModified")]
        public DateTime LastModified { get; set; }

        [Column("ModifiedBy")]
        [StringLength(100)]
        public string? ModifiedBy { get; set; }

        [Column("IsActive")]
        public bool IsActive { get; set; } = true;
    }
}