namespace MuseumServer.DTOs
{
    public class DocumentsReportRequest
    {
        public int? Limit { get; set; }
        public bool Compact { get; set; } = true;
    }

    public class ImagesReportRequest
    {
        public int? Limit { get; set; }
        public bool Compact { get; set; } = true;
        public bool IncludeImages { get; set; } = false;
    }

    public class VideosReportRequest
    {
        public int? Limit { get; set; }
        public bool Compact { get; set; } = true;
    }

    public class MuseumStatisticsRequest
    {
        public bool IncludeDepartments { get; set; } = true;
        public bool IncludeExhibits { get; set; } = true;
        public bool IncludeDocuments { get; set; } = true;
        public bool IncludeMediaFiles { get; set; } = true;
        public bool IncludeExhibitBreakdown { get; set; } = true;
    }

    public class LogsReportRequest
    {
        public int? Limit { get; set; }
    }

    public class CombinedReportRequest
    {
        public ExhibitsReportRequest? Exhibits { get; set; }
        public DocumentsReportRequest? Documents { get; set; }
        public ImagesReportRequest? Images { get; set; }
        public VideosReportRequest? Videos { get; set; }
        public MuseumStatisticsRequest? Statistics { get; set; }
        public LogsReportRequest? Logs { get; set; }
    }
}