using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MuseumServer.Attributes;
using MuseumServer.DTOs;
using MuseumServer.Models;
using MuseumServer.Services;

namespace MuseumServer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [SessionAuthorize]
    public class MediaFileController : ControllerBase
    {
        private readonly MediaFileService _service;
        private readonly IWebHostEnvironment _env;
        private readonly IFileService _fileService;
        private readonly ImageProcessor _imageProcessor;
        private readonly VideoProcessor _videoProcessor;
        private readonly LoggingService _logging;

        // Конструктор
        public MediaFileController(
            MediaFileService service,
            IWebHostEnvironment env,
            IFileService fileService,
            ImageProcessor imageProcessor,
            VideoProcessor videoProcessor,
            LoggingService logging)
        {
            _service = service;
            _env = env;
            _fileService = fileService;
            _imageProcessor = imageProcessor;
            _videoProcessor = videoProcessor;
            _logging = logging;
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
        public async Task<IActionResult> Create(
            [FromHeader] string token,
            [FromForm] CreateMediaFileRequest request)
        {
            if (request.File == null || request.File.Length == 0)
                return BadRequest(new { status = "error", message = "File is required" });

            var mediaType = GetMediaTypeFromPath(request.File.FileName);
            if (mediaType == null)
                return BadRequest(new { status = "error", message = "Unsupported media type" });

            var folder = mediaType switch
            {
                "image" => "media/images/original",
                "video" => "media/videos/original",
                _ => "media/other"
            };

            // Используем FileService
            var fileName = await _fileService.SaveFileAsync(request.File, folder);

            if (mediaType == "image")
            {
                var thumbnailFolder = "media/images/thumbnails";

                var thumbnailFullPath = Path.Combine(
                    _env.WebRootPath,
                    thumbnailFolder,
                    fileName
                );

                await _imageProcessor.SaveAsThumbnailAsync(
                    request.File,
                    thumbnailFullPath,
                    300,
                    300
                );
            }

            if (mediaType == "video")
            {
                var originalPath = Path.Combine(_env.WebRootPath, folder, fileName);

                // Thumbnail (jpg)
                var thumbnailFolder = "media/videos/thumbnails";
                var thumbnailPath = Path.Combine(
                    _env.WebRootPath,
                    thumbnailFolder,
                    Path.ChangeExtension(fileName, ".jpg")
                );

                await _videoProcessor.CreateThumbnailAsync(
                    originalPath,
                    thumbnailPath,
                    300
                );

                // Preview (mp4)
                var previewFolder = "media/videos/previews";
                var previewPath = Path.Combine(
                    _env.WebRootPath,
                    previewFolder,
                    Path.GetFileNameWithoutExtension(fileName) + "_preview.mp4"
                );

                await _videoProcessor.CreatePreviewAsync(
                    originalPath,
                    previewPath,
                    320,
                    4
                );
            }

            var media = new MediaFile
            {
                Title = request.Title,
                FilePath = $"{folder}/{fileName}",
                MediaType = mediaType,
                Description = request.Description,
                DepartmentId = request.DepartmentId
            };

            var created = await _service.CreateAsync(media);

            var entityType = mediaType == "image" ? "Image" : "Video";

            await _logging.LogAsync("admin", "Create", entityType, created.Title);

            return Ok(new { status = "ok", data = created });
        }

        // DELETE: api/MediaFiles/{id}
        [HttpDelete("{id}")]
        [SessionAuthorize(adminOnly: true)]
        public async Task<IActionResult> Delete([FromHeader] string token, int id)
        {
            var existing = await _service.GetEntityAsync(id);

            var deleted = await _service.DeleteAsync(id);
            if (!deleted)
                return NotFound(new { status = "error", message = "Media file not found" });

            var entityType = existing?.MediaType == "image" ? "Image" : "Video";

            await _logging.LogAsync("admin", "Delete", entityType, existing?.Title);

            return Ok(new { status = "ok" });
        }

        // GET: api/MediaFiles/count
        [HttpGet("count")]
        public async Task<IActionResult> GetCount([FromHeader] string token)
        {
            var mediaFiles = await _service.GetAllAsync();
            return Ok(new { status = "ok", count = mediaFiles.Count });
        }

        // GET: api/MediaFile/stream/{id}
        [HttpGet("stream/{id}")]
        public async Task<IActionResult> Stream(int id, [FromQuery] string? size = null, [FromQuery] string? token = null)
        {
            var media = await _service.GetEntityAsync(id);

            if (media == null || string.IsNullOrEmpty(media.FilePath))
                return NotFound(new { status = "error", message = "Media file not found" });

            string relativePath = media.FilePath;

            // Если запрощен thumbnail
            // Image thumbnail
            if (media.MediaType == "image" && size == "thumb")
            {
                relativePath = media.FilePath
                    .Replace("original", "thumbnails");
            }

            // Video thumbnail
            if (media.MediaType == "video" && size == "thumb")
            {
                relativePath = media.FilePath
                    .Replace("original", "thumbnails");

                relativePath = Path.ChangeExtension(relativePath, ".jpg");
            }

            // Video preview
            if (media.MediaType == "video" && size == "preview")
            {
                relativePath = media.FilePath
                    .Replace("original", "previews");

                relativePath = Path.Combine(
                    Path.GetDirectoryName(relativePath)!,
                    Path.GetFileNameWithoutExtension(relativePath) + "_preview.mp4"
                );
            }

            var path = Path.IsPathRooted(relativePath)
                ? relativePath
                : Path.Combine(_env.WebRootPath, relativePath);

            if (!System.IO.File.Exists(path))
                return NotFound(new { status = "error", message = "File not found on disk" });

            var contentType = GetContentType(relativePath);

            // кеш отключаем
            Response.Headers["Cache-Control"] = "no-store, no-cache, must-revalidate";
            Response.Headers["Pragma"] = "no-cache";
            Response.Headers["Expires"] = "0";

            return PhysicalFile(path, contentType, enableRangeProcessing: true);
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

        // Определения MIME-типа (Content-Type) файла по его расширению.
        private string GetContentType(string path)
        {
            var ext = Path.GetExtension(path)?.ToLowerInvariant();

            return ext switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",

                ".mp4" => "video/mp4",
                ".mov" => "video/quicktime",
                ".avi" => "video/x-msvideo",

                _ => "application/octet-stream"
            };
        }
    }
}