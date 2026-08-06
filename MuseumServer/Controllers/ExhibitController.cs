using Microsoft.AspNetCore.Mvc;
using MuseumServer.DTOs;
using MuseumServer.Models;
using MuseumServer.Services;
using MuseumServer.Attributes;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace MuseumServer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [SessionAuthorize]

    public class ExhibitController : ControllerBase
    {
        private readonly ExhibitService _service;
        private readonly IFileService _fileService;
        private readonly ImageProcessor _imageProcessor;
        private readonly LoggingService _logging;

        public ExhibitController(
            ExhibitService service,
            IFileService fileService,
            ImageProcessor imageProcessor,
            LoggingService logging)
        {
            _service = service;
            _fileService = fileService;
            _imageProcessor = imageProcessor;
            _logging = logging;
        }


        // GET: api/exhibit
        [HttpGet]
        public async Task<IActionResult> GetAll([FromHeader] string token)
        {
            var exhibits = await _service.GetAllExhibitsAsync();
            return Ok(new { status = "ok", data = exhibits });
        }

        // GET: api/exhibit/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> Get([FromHeader] string token, int id)
        {
            var exhibit = await _service.GetExhibitAsync(id);
            if (exhibit == null)
                return NotFound(new { status = "error", message = "Exhibit not found" });

            return Ok(new { status = "ok", data = exhibit });
        }

        // GET: api/exhibit/count
        [HttpGet("count")]
        public async Task<IActionResult> GetCount([FromHeader] string token)
        {
            var count = await _service.GetExhibitCountAsync();
            return Ok(new { status = "ok", count });
        }

        // POST: api/exhibit
        [HttpPost]
        [SessionAuthorize(adminOnly: true)]
        public async Task<IActionResult> Create(
            [FromHeader] string token,
            [FromForm] CreateExhibitRequest request)
        {
            if (request.Image == null || request.Image.Length == 0)
                return BadRequest(new { status = "error", message = "Image is required" });

            var ext = Path.GetExtension(request.Image.FileName)?.ToLowerInvariant();
            var allowed = new[] { ".jpg", ".jpeg", ".png", ".webp", ".gif" };

            if (ext == null || !allowed.Contains(ext))
                return BadRequest(new { status = "error", message = "Unsupported image type" });

            // 📁 сохраняем оригинал
            var fileName = await _fileService.SaveFileAsync(request.Image, "exhibits/images");

            // 📁 путь до thumbnail
            var thumbFolder = Path.Combine("exhibits", "thumbnails");
            var thumbFullPath = Path.Combine(
                Directory.GetCurrentDirectory(),
                "wwwroot",
                thumbFolder,
                fileName
            );

            // Создаём thumbnail
            await _imageProcessor.SaveAsThumbnailAsync(
                request.Image,
                thumbFullPath,
                width: 300,
                height: 300
            );

            var exhibit = new Exhibit
            {
                Name = request.Name,
                Description = request.Description,
                Materials = request.Materials,
                IsPermanent = request.IsPermanent,
                DepartmentId = request.DepartmentId,
                ImagePath = fileName
            };

            var created = await _service.CreateExhibitAsync(exhibit);

            await _logging.LogAsync("admin", "Create", "Exhibit", created.Name);

            return Ok(new { status = "ok", data = created });
        }

        // PUT: api/exhibit/{id}
        [HttpPut("{id}")]
        [SessionAuthorize(adminOnly: true)]
        public async Task<IActionResult> Update(
            [FromHeader] string token,
            int id,
            [FromForm] UpdateExhibitRequest request)
        {
            var updated = await _service.UpdateExhibitAsync(id, request);

            if (updated == null)
                return NotFound(new { status = "error", message = "Exhibit not found" });

            await _logging.LogAsync("admin", "Update", "Exhibit", updated.Name);

            return Ok(new { status = "ok", data = updated });
        }

        // DELETE: api/exhibit/{id}
        [HttpDelete("{id}")]
        [SessionAuthorize(adminOnly: true)]
        public async Task<IActionResult> Delete([FromHeader] string token, int id)
        {
            var existing = await _service.GetExhibitEntityAsync(id);

            var deleted = await _service.DeleteExhibitAsync(id);
            if (!deleted)
                return NotFound(new { status = "error", message = "Exhibit not found" });

            await _logging.LogAsync("admin", "Delete", "Exhibit", existing?.Name);

            return Ok(new { status = "ok" });
        }

        // GET: api/exhibit/image/{id}
        [HttpGet("image/{id}")]
        public async Task<IActionResult> GetImage([FromHeader] string token, int id)
        {
            var exhibit = await _service.GetExhibitEntityAsync(id);

            if (exhibit == null || string.IsNullOrEmpty(exhibit.ImagePath))
                return NotFound(new { status = "error", message = "Image not found" });

            var basePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            var imagesRoot = Path.Combine(basePath, "exhibits", "images");

            var path = Path.IsPathRooted(exhibit.ImagePath)
                ? exhibit.ImagePath
                : Path.Combine(imagesRoot, exhibit.ImagePath);

            var fullPath = Path.GetFullPath(path);

            // защита от выхода за папку images
            if (!fullPath.StartsWith(imagesRoot))
                return BadRequest(new { status = "error", message = "Invalid file path" });

            if (!System.IO.File.Exists(fullPath))
                return NotFound(new { status = "error", message = "File not found on disk" });

            var contentType = GetImageContentType(fullPath);

            return PhysicalFile(fullPath, contentType, enableRangeProcessing: true);
        }

        // GET: api/exhibit/thumbnail/{id}
        [HttpGet("thumbnail/{id}")]
        public async Task<IActionResult> GetThumbnail([FromHeader] string token, int id)
        {
            var exhibit = await _service.GetExhibitEntityAsync(id);

            if (exhibit == null || string.IsNullOrEmpty(exhibit.ImagePath))
                return NotFound(new { status = "error", message = "Thumbnail not found" });

            var basePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            var thumbsRoot = Path.Combine(basePath, "exhibits", "thumbnails");

            var path = Path.Combine(thumbsRoot, exhibit.ImagePath);
            var fullPath = Path.GetFullPath(path);

            // защита
            if (!fullPath.StartsWith(thumbsRoot))
                return BadRequest(new { status = "error", message = "Invalid file path" });

            if (!System.IO.File.Exists(fullPath))
                //return NotFound(new { status = "error", message = "Thumbnail file not found" });
                // fallback на оригинал
                return await GetImage(token, id);

            var contentType = GetImageContentType(fullPath);

            return PhysicalFile(fullPath, contentType, enableRangeProcessing: true);
        }

        // Определение типа данных
        private string GetImageContentType(string path)
        {
            var ext = Path.GetExtension(path)?.ToLowerInvariant();

            return ext switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".webp" => "image/webp",
                ".gif" => "image/gif",
                _ => "application/octet-stream"
            };
        }
    }
}