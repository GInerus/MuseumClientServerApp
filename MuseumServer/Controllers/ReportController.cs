using Microsoft.AspNetCore.Mvc;
using MuseumServer.Attributes;
using MuseumServer.DTOs;
using MuseumServer.Services;

namespace MuseumServer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [SessionAuthorize(adminOnly: true)]
    public class ReportController : ControllerBase
    {
        private readonly ReportService _service;
        private readonly LoggingService _logging;

        public ReportController(ReportService service, LoggingService logging)
        {
            _service = service;
            _logging = logging;
        }

        // GET: api/Report/exhibits/count
        [HttpGet("exhibits/count")]
        public async Task<IActionResult> GetExhibitsCount([FromHeader] string token)
        {
            var count = await _service.GetExhibitsCountAsync();
            return Ok(new { status = "ok", count });
        }

        // POST: api/Report/exhibits
        [HttpPost("exhibits")]
        public async Task<IActionResult> GenerateExhibitsReport(
            [FromHeader] string token,
            [FromBody] ExhibitsReportRequest request)
        {
            var pdfBytes = await _service.GenerateExhibitsReportAsync(request);

            await _logging.LogAsync("admin", "GenerateReport", "Report", "Отчёт по экспонатам");

            return File(pdfBytes, "application/pdf", "exhibits_report.pdf");
        }
    }
}