namespace MuseumServer.DTOs
{
    public class ExhibitsReportRequest
    {
        public int? Limit { get; set; }
        public bool Compact { get; set; } = true;
        public bool IncludeImages { get; set; } = false;
    }
}