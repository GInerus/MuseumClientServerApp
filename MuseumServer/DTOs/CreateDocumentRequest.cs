namespace MuseumServer.DTOs
{
    public class CreateDocumentRequest
    {
        public string Title { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;

        // Привязка к экспонату (может быть null)
        public int? ExhibitId { get; set; }

        // Привязка к отделу (может быть null)
        public int? DepartmentId { get; set; }
    }
}