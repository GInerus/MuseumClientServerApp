namespace MuseumServer.DTOs
{
    public class CreateDocumentRequest
    {
        public string FilePath { get; set; } = string.Empty;

        // Привязка к экспонату (может быть null)
        public int? ExhibitId { get; set; }

        // Привязка к отделу (обязательна)
        public int DepartmentId { get; set; }
    }
}