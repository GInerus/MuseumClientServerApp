namespace MuseumServer.DTOs
{
    public class CreateMediaFileRequest
    {
        public string FilePath { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int DepartmentId { get; set; }
    }
}