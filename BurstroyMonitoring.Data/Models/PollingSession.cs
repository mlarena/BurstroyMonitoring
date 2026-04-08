using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BurstroyMonitoring.Data.Models
{
    [Table("PollingSessions")]
    public class PollingSession
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public int MonitoringPostId { get; set; }

        [Required]
        public DateTime StartedAt { get; set; } = DateTime.UtcNow;

        public DateTime? CompletedAt { get; set; }

        [Required]
        [MaxLength(20)]
        public string Status { get; set; } = "IN_PROGRESS";

        [Required]
        public int TotalSensorsCount { get; set; }

        [Required]
        public int SuccessfulSensorsCount { get; set; } = 0;

        [Column(TypeName = "jsonb")]
        public string? FailedSensorsDetails { get; set; }

        [ForeignKey("MonitoringPostId")]
        public virtual MonitoringPost MonitoringPost { get; set; } = null!;
    }
}
