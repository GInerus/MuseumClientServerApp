namespace MuseumServer.DTOs
{
    public class CreateExhibitRequest
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Materials { get; set; }
        public bool IsPermanent { get; set; } = true;
        public string? ImagePath { get; set; }
        public int DepartmentId { get; set; }
    }
}