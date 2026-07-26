using System.ComponentModel.DataAnnotations;

namespace MuseumServer.Models
{
    public class LogEntry
    {
        [Key]
        public int LogId { get; set; }

        [Required]
        [MaxLength(20)]
        public string UserType { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string Action { get; set; } = string.Empty;

        [MaxLength(50)]
        public string? EntityType { get; set; }

        [MaxLength(300)]
        public string? EntityName { get; set; }

        public DateTime Timestamp { get; set; }
    }
}