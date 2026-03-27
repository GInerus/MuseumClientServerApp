namespace MuseumServer.DTOs
{
    // Для создания нового отдела
    public class CreateDepartmentRequest
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? ImagePath { get; set; }
    }
}
