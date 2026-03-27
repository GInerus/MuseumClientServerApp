using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MuseumServer.Models
{
    public class Document
    {
        [Key]
        public int DocumentId { get; private set; }  // автоинкремент

        [Required]
        [MaxLength(500)]
        public string FilePath { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        public string FileType { get; set; } = string.Empty; // pdf, doc, docx, txt, md

        // Связь с экспонатом (может быть null)
        public int? ExhibitId { get; set; }

        public Exhibit? Exhibit { get; set; }

        // Связь с отделом (обязательна)
        [Required]
        public int DepartmentId { get; set; }
        public Department Department { get; set; } = null!;
    }
}