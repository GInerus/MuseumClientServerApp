namespace MuseumServer.DTOs
{
    public class MediaFileResponse
    {
        public int MediaFileId { get; set; } // Автоинкремент
        public string FilePath { get; set; } = string.Empty;
        public string MediaType { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DepartmentInfo Department { get; set; } = null!;
    }
}