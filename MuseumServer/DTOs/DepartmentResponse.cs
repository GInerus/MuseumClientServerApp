namespace MuseumServer.DTOs
{
    // Ответ при выводе отдела (id)
    public class DepartmentResponse
    {
        public int DepartmentId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? ImagePath { get; set; }
    }
}
