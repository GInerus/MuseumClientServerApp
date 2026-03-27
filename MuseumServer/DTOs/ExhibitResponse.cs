namespace MuseumServer.DTOs
{
    public class ExhibitResponse
    {
        public int ExhibitId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Materials { get; set; }
        public bool IsPermanent { get; set; }
        public string? ImagePath { get; set; }
        public int DepartmentId { get; set; }
    }
}