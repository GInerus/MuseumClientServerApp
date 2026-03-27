using MuseumServer.Models;
using System.Text.Json.Serialization;

public class Exhibit
{
    public int ExhibitId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Materials { get; set; }
    public bool IsPermanent { get; set; } = true;
    public string? ImagePath { get; set; }
    public int DepartmentId { get; set; }

    [JsonIgnore]  // Не включает в JSON при POST
    public Department? Department { get; set; }
}