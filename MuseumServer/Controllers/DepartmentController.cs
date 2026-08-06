using Microsoft.AspNetCore.Mvc;
using MuseumServer.Attributes;
using MuseumServer.DTOs;
using MuseumServer.Services;

namespace MuseumServer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DepartmentController : ControllerBase
    {
        private readonly DepartmentService _service;
        private readonly IFileService _fileService;
        private readonly ImageProcessor _imageProcessor;
        private readonly LoggingService _logging;

        public DepartmentController(
            DepartmentService service,
            IFileService fileService,
            ImageProcessor imageProcessor,
            LoggingService logging)
        {
            _service = service;
            _fileService = fileService;
            _imageProcessor = imageProcessor;
            _logging = logging;
        }

        // GET: api/department/count
        [HttpGet("count")]
        [SessionAuthorize]
        public async Task<IActionResult> GetCount([FromHeader] string token)
        {
            var count = await _service.GetCountAsync();
            return Ok(new { status = "ok", count });
        }

        // GET: api/department
        [HttpGet]
        [SessionAuthorize]
        public async Task<IActionResult> GetAll([FromHeader] string token)
        {
            var depts = await _service.GetAllAsync();
            return Ok(new { status = "ok", data = depts });
        }

        // GET: api/department/{id}
        [HttpGet("{id}")]
        [SessionAuthorize]
        public async Task<IActionResult> GetContent([FromHeader] string token, int id)
        {
            var content = await _service.GetContentAsync(id);

            if (content == null)
                return NotFound(new
                {
                    status = "error",
                    message = "Department not found"
                });

            return Ok(new
            {
                status = "ok",
                data = content
            });
        }

        // POST /api/Department
        [HttpPost]
        [SessionAuthorize(adminOnly: true)]
        public async Task<IActionResult> Create(
            [FromHeader] string token,
            [FromForm] CreateDepartmentRequest request)
        {
            string? imageName = null;

            if (request.Image != null)
            {
                // 1. сохраняем оригинал через FileService (он генерит имя)
                imageName = await _fileService.SaveFileAsync(
                    request.Image,
                    "departments/images"
                );

                // 2. строим полный путь к сохранённому файлу
                var fullPath = Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "wwwroot",
                    "departments",
                    "images",
                    imageName
                );

                // 3. делаем thumbnail В ТОТ ЖЕ ФАЙЛ (или можно перезаписать)
                await _imageProcessor.SaveAsThumbnailAsync(
                    request.Image,
                    fullPath,
                    200,
                    200
                );
            }

            var dept = await _service.CreateAsync(request, imageName);

            await _logging.LogAsync("admin", "Create", "Department", dept.Name);

            return Ok(new
            {
                status = "ok",
                data = dept
            });
        }

        // DELETE: api/department/{id}
        [HttpDelete("{id}")]
        [SessionAuthorize(adminOnly: true)]
        public async Task<IActionResult> Delete(
            [FromHeader] string token,
            int id)
        {
            var existing = await _service.GetByIdAsync(id);

            var deleted = await _service.DeleteAsync(id);

            if (!deleted)
                return NotFound(new
                {
                    status = "error",
                    message = "Department not found"
                });

            await _logging.LogAsync("admin", "Delete", "Department", existing?.Name);

            return Ok(new { status = "ok" });
        }

        // PUT: api/department/{id}
        [HttpPut("{id}")]
        [SessionAuthorize(adminOnly: true)]
        public async Task<IActionResult> Update(
        [FromHeader] string token,
        int id,
        [FromForm] UpdateDepartmentRequest request)
        {
            var updated = await _service.UpdateAsync(id, request);

            if (!updated)
                return NotFound(new
                {
                    status = "error",
                    message = "Department not found"
                });

            var dept = await _service.GetByIdAsync(id);

            await _logging.LogAsync("admin", "Update", "Department", dept?.Name);

            return Ok(new { status = "ok" });
        }

        // GET: api/department/image/{id}
        [HttpGet("image/{id}")]
        [SessionAuthorize]
        public async Task<IActionResult> GetImage(
            [FromHeader] string token,
            int id)
        {
            var dept = await _service.GetByIdAsync(id);

            if (dept == null || string.IsNullOrEmpty(dept.ImagePath))
                return NotFound(new
                {
                    status = "error",
                    message = "Image not found"
                });

            var basePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            var imagesRoot = Path.Combine(basePath, "departments", "images");

            var fullPath = Path.Combine(imagesRoot, dept.ImagePath);
            fullPath = Path.GetFullPath(fullPath);

            // 🔒 защита от выхода за папку
            if (!fullPath.StartsWith(imagesRoot))
                return BadRequest(new
                {
                    status = "error",
                    message = "Invalid file path"
                });

            if (!System.IO.File.Exists(fullPath))
                return NotFound(new
                {
                    status = "error",
                    message = "File not found on disk"
                });

            var contentType = GetImageContentType(fullPath);

            return PhysicalFile(fullPath, contentType, enableRangeProcessing: true);
        }

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