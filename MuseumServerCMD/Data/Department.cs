using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MuseumServer.Data
{
    public class Department
    {
        [Key]
        public int DepartmentId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public List<Exhibit> Exhibits { get; set; } = new List<Exhibit>();
    }

    public class Exhibit
    {
        [Key]
        public int ExhibitId { get; set; }
        public int DepartmentId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string ImagePath { get; set; }
    }
}