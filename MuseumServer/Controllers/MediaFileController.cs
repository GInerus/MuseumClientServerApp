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
    public class MediaFileController : ControllerBase
    {
        private readonly MediaFileService _service;

        public MediaFileController(MediaFileService service)
        {
            _service = service;
        }

        // GET: api/MediaFiles
        [HttpGet]
        public async Task<IActionResult> GetAll([FromHeader] string token)
        {
            var mediaFiles = await _service.GetAllAsync();
            return Ok(new { status = "ok", data = mediaFiles });
        }

        // GET: api/MediaFiles/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> Get([FromHeader] string token, int id)
        {
            var media = await _service.GetAsync(id);
            if (media == null)
                return NotFound(new { status = "error", message = "Media file not found" });

            return Ok(new { status = "ok", data = media });
        }

        // POST: api/MediaFiles
        [HttpPost]
        [SessionAuthorize(adminOnly: true)]
        public async Task<IActionResult> Create([FromHeader] string token, [FromBody] CreateMediaFileRequest request)
        {
            // Определяем тип медиа по расширению файла
            string? mediaType = GetMediaTypeFromPath(request.FilePath);
            if (mediaType == null)
            {
                return BadRequest(new { status = "error", message = "Unsupported media type" });
            }

            var media = new MediaFile
            {
                FilePath = request.FilePath,
                MediaType = mediaType,
                Description = request.Description,
                DepartmentId = request.DepartmentId
            };

            var created = await _service.CreateAsync(media);
            return Ok(new { status = "ok", data = created });
        }

        // DELETE: api/MediaFiles/{id}
        [HttpDelete("{id}")]
        [SessionAuthorize(adminOnly: true)]
        public async Task<IActionResult> Delete([FromHeader] string token, int id)
        {
            var deleted = await _service.DeleteAsync(id);
            if (!deleted)
                return NotFound(new { status = "error", message = "Media file not found" });

            return Ok(new { status = "ok" });
        }

        // GET: api/MediaFiles/count
        [HttpGet("count")]
        public async Task<IActionResult> GetCount([FromHeader] string token)
        {
            var mediaFiles = await _service.GetAllAsync();
            return Ok(new { status = "ok", count = mediaFiles.Count });
        }

        // Вспомогательный метод для определения типа медиа
        private string? GetMediaTypeFromPath(string path)
        {
            var ext = Path.GetExtension(path)?.ToLowerInvariant().TrimStart('.');
            if (ext == null) return null;

            return ext switch
            {
                "jpg" => "image",
                "jpeg" => "image",
                "png" => "image",
                "gif" => "image",
                "mp4" => "video",
                "mov" => "video",
                "avi" => "video",
                _ => null
            };
        }
    }
}