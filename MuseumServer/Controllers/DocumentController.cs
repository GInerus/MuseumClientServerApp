using Microsoft.AspNetCore.Mvc;
using MuseumServer.Models;
using MuseumServer.Services;
using MuseumServer.DTOs;
using MuseumServer.Attributes;

namespace MuseumServer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [SessionAuthorize]
    public class DocumentController : ControllerBase
    {
        private readonly DocumentService _service;
        private readonly IFileService _fileService;
        private readonly LoggingService _logging;

        public DocumentController(DocumentService service, IFileService fileService, LoggingService logging)
        {
            _service = service;
            _fileService = fileService;
            _logging = logging;
        }

        // GET: api/document
        [HttpGet]
        public async Task<IActionResult> GetAll([FromHeader] string token)
        {
            var documents = await _service.GetAllDocumentsAsync();
            return Ok(new { status = "ok", data = documents });
        }

        // GET: api/document/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> Get([FromHeader] string token, int id)
        {
            var document = await _service.GetDocumentAsync(id);
            if (document == null)
                return NotFound(new { status = "error", message = "Document not found" });

            return Ok(new { status = "ok", data = document });
        }

        // GET: api/document/count
        [HttpGet("count")]
        public async Task<IActionResult> GetCount([FromHeader] string token)
        {
            var count = await _service.GetDocumentCountAsync();
            return Ok(new { status = "ok", count });
        }

        // POST: api/document
        [HttpPost]
        [SessionAuthorize(adminOnly: true)]
        public async Task<IActionResult> Create([FromHeader] string token, [FromForm] CreateDocumentRequest request)
        {
            try
            {
                var document = await _service.CreateDocumentAsync(request);

                await _logging.LogAsync("admin", "Create", "Document", document.Title);

                return Ok(new { status = "ok", data = document });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { status = "error", message = ex.Message });
            }
        }

        // DELETE: api/document/{id}
        [HttpDelete("{id}")]
        [SessionAuthorize(adminOnly: true)]
        public async Task<IActionResult> Delete([FromHeader] string token, int id)
        {
            var existing = await _service.GetDocumentEntityAsync(id);

            var deleted = await _service.DeleteDocumentWithFileAsync(id);

            if (!deleted)
                return NotFound(new { status = "error", message = "Document not found" });

            await _logging.LogAsync("admin", "Delete", "Document", existing?.Title);

            return Ok(new { status = "ok" });
        }

        // GET: api/document/stream/{id}
        [HttpGet("stream/{id}")]
        public async Task<IActionResult> Stream([FromHeader] string token, int id)
        {
            var document = await _service.GetDocumentEntityAsync(id);

            if (document == null || string.IsNullOrEmpty(document.FilePath))
                return NotFound(new { status = "error", message = "Document not found" });

            var basePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");

            var path = Path.IsPathRooted(document.FilePath)
                ? document.FilePath
                : Path.Combine(basePath, document.FilePath);

            var fullPath = Path.GetFullPath(path);

            if (!fullPath.StartsWith(basePath))
                return BadRequest(new { status = "error", message = "Invalid file path" });

            if (!System.IO.File.Exists(fullPath))
                return NotFound(new { status = "error", message = "File not found on disk" });

            var contentType = GetContentType(fullPath);

            return PhysicalFile(fullPath, contentType, enableRangeProcessing: true);
        }

        // ===== Helpers =====

        private string GetContentType(string path)
        {
            var ext = Path.GetExtension(path)?.ToLowerInvariant();

            return ext switch
            {
                ".pdf" => "application/pdf",

                ".doc" => "application/msword",
                ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",

                ".txt" => "text/plain",
                ".md" => "text/markdown",

                _ => "application/octet-stream"
            };
        }
    }
}