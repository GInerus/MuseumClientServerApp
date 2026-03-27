namespace MuseumServer.Models
{
    public class Exhibit
    {
        public int ExhibitId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Materials { get; set; }
        public bool IsPermanent { get; set; } = true;
        public int DepartmentId { get; set; }

        // Навигационное свойство
        public Department? Department { get; set; }
    }
}