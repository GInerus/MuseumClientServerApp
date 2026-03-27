using System.ComponentModel.DataAnnotations;

namespace MuseumServer.Models
{
    public class MediaFile
    {
        [Key]
        public int MediaFileId { get; private set; }

        [Required]
        [MaxLength(500)]
        public string FilePath { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        public string MediaType { get; set; } = string.Empty; // video, image

        [MaxLength(300)]
        public string? Description { get; set; }

        [Required]
        public int DepartmentId { get; set; }
        public Department Department { get; set; } = null!;
    }
}