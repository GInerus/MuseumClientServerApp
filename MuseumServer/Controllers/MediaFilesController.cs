using Microsoft.AspNetCore.Mvc;
using MuseumServer.Models;
using MuseumServer.Services;
using MuseumServer.Attributes;

namespace MuseumServer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [SessionAuthorize]
    public class MediaFilesController : ControllerBase
    {
        private readonly MediaFileService _service;

        public MediaFilesController(MediaFileService service)
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
        public async Task<IActionResult> Create([FromHeader] string token, [FromBody] MediaFile request)
        {
            // Проверяем тип медиа
            if (request.MediaType != "image" && request.MediaType != "video")
            {
                return BadRequest(new { status = "error", message = "Invalid media type" });
            }

            var media = new MediaFile
            {
                FilePath = request.FilePath,
                MediaType = request.MediaType,
                Description = request.Description,
                DepartmentId = request.DepartmentId
            };

            var created = await _service.CreateAsync(media);
            return Ok(new { status = "ok", data = created });
        }

        // DELETE: api/MediaFiles/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete([FromHeader] string token, int id)
        {
            var deleted = await _service.DeleteAsync(id);
            if (!deleted)
                return NotFound(new { status = "error", message = "Media file not found" });

            return Ok(new { status = "ok" });
        }
    }
}