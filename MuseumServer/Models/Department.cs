namespace MuseumServer.Models
{
    public class Department
    {
        public int DepartmentId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? ImagePath { get; set; }

        // Навигационное свойство — список экспонатов в этом отделе
        public List<Exhibit>? Exhibits { get; set; }
    }
}