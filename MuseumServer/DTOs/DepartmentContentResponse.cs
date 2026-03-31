namespace MuseumServer.DTOs
{
    // Ответ с контентом отдела
    public class DepartmentContentResponse
    {
        public DepartmentResponse Department { get; set; } = new DepartmentResponse();
        public List<ExhibitResponse> Exhibits { get; set; } = new List<ExhibitResponse>();
        public List<MediaFileResponse> MediaFiles { get; set; } = new List<MediaFileResponse>();
        public List<DocumentFullResponse> Documents { get; set; } = new List<DocumentFullResponse>();
    }
}
