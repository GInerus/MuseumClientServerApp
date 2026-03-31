namespace MuseumServer.DTOs
{
    public class DocumentResponse
    {
        public int DocumentId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public string FileType { get; set; } = string.Empty;
        public int? ExhibitId { get; set; }

        // Минимальная информация об отделе
        public DepartmentInfo? Department { get; set; }
    }
}