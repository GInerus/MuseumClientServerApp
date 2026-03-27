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

        public DocumentController(DocumentService service)
        {
            _service = service;
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
        public async Task<IActionResult> Create([FromHeader] string token, [FromBody] CreateDocumentRequest request)
        {
            // Определяем тип файла по расширению
            string fileType = GetFileTypeFromPath(request.FilePath);
            if (fileType == null)
                return BadRequest(new { status = "error", message = "Unsupported file type" });

            var document = new Document
            {
                FilePath = request.FilePath,
                FileType = fileType,
                ExhibitId = request.ExhibitId,
                DepartmentId = request.DepartmentId
            };

            var created = await _service.CreateDocumentAsync(document);
            return Ok(new { status = "ok", data = created });
        }

        // DELETE: api/document/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete([FromHeader] string token,int id)
        {
            var deleted = await _service.DeleteDocumentAsync(id);
            if (!deleted)
                return NotFound(new { status = "error", message = "Document not found" });

            return Ok(new { status = "ok" });
        }

        // Вспомогательный метод для определения типа файла
        private string? GetFileTypeFromPath(string path)
        {
            var ext = Path.GetExtension(path)?.ToLowerInvariant().TrimStart('.');
            if (ext == null) return null;

            // Проверяем, поддерживается ли тип
            return ext switch
            {
                "pdf" => "pdf",
                "doc" => "doc",
                "docx" => "docx",
                "txt" => "txt",
                "md" => "md",
                _ => null
            };
        }
    }
}