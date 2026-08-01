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

        // ========== ЭКСПОНАТЫ ==========
        [HttpPost("exhibits")]
        public async Task<IActionResult> GenerateExhibitsReport(
            [FromHeader] string token,
            [FromBody] ExhibitsReportRequest request)
        {
            var pdfBytes = await _service.GenerateExhibitsReportAsync(request);
            await _logging.LogAsync("admin", "GenerateReport", "Report", "Отчёт по экспонатам");
            return File(pdfBytes, "application/pdf", "exhibits_report.pdf");
        }

        // ========== ДОКУМЕНТЫ ==========
        [HttpPost("documents")]
        public async Task<IActionResult> GenerateDocumentsReport(
            [FromHeader] string token,
            [FromBody] DocumentsReportRequest request)
        {
            var pdfBytes = await _service.GenerateDocumentsReportAsync(request);
            await _logging.LogAsync("admin", "GenerateReport", "Report", "Отчёт по документам");
            return File(pdfBytes, "application/pdf", "documents_report.pdf");
        }

        // ========== ИЗОБРАЖЕНИЯ ==========
        [HttpGet("images/count")]
        public async Task<IActionResult> GetImagesCount([FromHeader] string token)
        {
            var count = await _service.GetImagesCountAsync();
            return Ok(new { status = "ok", count });
        }

        [HttpPost("images")]
        public async Task<IActionResult> GenerateImagesReport(
            [FromHeader] string token,
            [FromBody] ImagesReportRequest request)
        {
            var pdfBytes = await _service.GenerateImagesReportAsync(request);
            await _logging.LogAsync("admin", "GenerateReport", "Report", "Отчёт по изображениям");
            return File(pdfBytes, "application/pdf", "images_report.pdf");
        }

        // ========== ВИДЕО ==========
        [HttpGet("videos/count")]
        public async Task<IActionResult> GetVideosCount([FromHeader] string token)
        {
            var count = await _service.GetVideosCountAsync();
            return Ok(new { status = "ok", count });
        }

        [HttpPost("videos")]
        public async Task<IActionResult> GenerateVideosReport(
            [FromHeader] string token,
            [FromBody] VideosReportRequest request)
        {
            var pdfBytes = await _service.GenerateVideosReportAsync(request);
            await _logging.LogAsync("admin", "GenerateReport", "Report", "Отчёт по видео");
            return File(pdfBytes, "application/pdf", "videos_report.pdf");
        }

        // ========== СТАТИСТИКА ==========
        [HttpPost("statistics")]
        public async Task<IActionResult> GenerateMuseumStatistics(
            [FromHeader] string token,
            [FromBody] MuseumStatisticsRequest request)
        {
            var pdfBytes = await _service.GenerateMuseumStatisticsAsync(request);
            await _logging.LogAsync("admin", "GenerateReport", "Report", "Статистика музея");
            return File(pdfBytes, "application/pdf", "museum_statistics.pdf");
        }

        // ========== ЖУРНАЛ ДЕЙСТВИЙ ==========
        [HttpGet("logs/count")]
        public async Task<IActionResult> GetLogsCount([FromHeader] string token)
        {
            var count = await _service.GetLogsCountAsync();
            return Ok(new { status = "ok", count });
        }

        [HttpPost("logs")]
        public async Task<IActionResult> GenerateLogsReport(
            [FromHeader] string token,
            [FromBody] LogsReportRequest request)
        {
            var pdfBytes = await _service.GenerateLogsReportAsync(request);
            await _logging.LogAsync("admin", "GenerateReport", "Report", "Журнал действий");
            return File(pdfBytes, "application/pdf", "logs_report.pdf");
        }

        // ========== СВОДНЫЙ ОТЧЁТ ==========
        [HttpPost("combined")]
        public async Task<IActionResult> GenerateCombinedReport(
            [FromHeader] string token,
            [FromBody] CombinedReportRequest request)
        {
            var pdfBytes = await _service.GenerateCombinedReportAsync(request);
            await _logging.LogAsync("admin", "GenerateReport", "Report", "Сводный отчёт");
            return File(pdfBytes, "application/pdf", $"combined_report_{DateTime.Now:yyyyMMdd_HHmm}.pdf");
        }
    }
}