using Microsoft.AspNetCore.Mvc;
using MuseumServer.Attributes;
using MuseumServer.DTOs;
using MuseumServer.Services;

namespace MuseumServer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [SessionAuthorize(adminOnly: true)]
    public class BackupController : ControllerBase
    {
        private readonly BackupService _service;
        private readonly LoggingService _logging;

        public BackupController(BackupService service, LoggingService logging)
        {
            _service = service;
            _logging = logging;
        }

        [HttpGet("settings")]
        public async Task<IActionResult> GetSettings([FromHeader] string token)
        {
            var settings = await _service.GetSettingsAsync();
            return Ok(new { status = "ok", data = settings });
        }

        [HttpPut("settings")]
        public async Task<IActionResult> UpdateSettings(
            [FromHeader] string token,
            [FromBody] UpdateBackupSettingsRequest request)
        {
            var settings = await _service.UpdateSettingsAsync(request);
            await _logging.LogAsync("admin", "Update", "BackupSettings", "Настройки резервного копирования");
            return Ok(new { status = "ok", data = settings });
        }

        [HttpGet]
        public async Task<IActionResult> GetBackups([FromHeader] string token)
        {
            var backups = await _service.GetBackupsAsync();
            return Ok(new { status = "ok", data = backups });
        }

        [HttpPost("full")]
        public async Task<IActionResult> CreateFull([FromHeader] string token)
        {
            var result = await _service.CreateFullBackupAsync("admin");
            return Ok(new { status = "ok", data = result });
        }

        [HttpPost("differential")]
        public async Task<IActionResult> CreateDifferential([FromHeader] string token)
        {
            try
            {
                var result = await _service.CreateDifferentialBackupAsync("admin");
                return Ok(new { status = "ok", data = result });
            }
            catch (InvalidOperationException ex) when (ex.Message == "FULL_BACKUP_REQUIRED")
            {
                return BadRequest(new
                {
                    status = "error",
                    message = "FULL_BACKUP_REQUIRED"
                });
            }
        }

        [HttpPost("restore")]
        public async Task<IActionResult> Restore(
            [FromHeader] string token,
            [FromBody] RestoreBackupRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.BaseName))
            {
                return BadRequest(new
                {
                    status = "error",
                    message = "BASE_NAME_REQUIRED"
                });
            }

            try
            {
                await _service.RestoreBackupAsync("admin", request.BaseName, request.RestoreMedia);
                return Ok(new { status = "ok" });
            }
            catch (FileNotFoundException ex) when (ex.Message == "BACKUP_NOT_FOUND"
                || ex.Message == "DATABASE_BACKUP_NOT_FOUND"
                || ex.Message == "BASE_FULL_BACKUP_NOT_FOUND")
            {
                return NotFound(new { status = "error", message = ex.Message });
            }
        }

        [HttpDelete("{baseName}")]
        public async Task<IActionResult> Delete([FromHeader] string token, string baseName)
        {
            if (string.IsNullOrWhiteSpace(baseName))
            {
                return BadRequest(new
                {
                    status = "error",
                    message = "BASE_NAME_REQUIRED"
                });
            }

            await _service.DeleteBackupAsync("admin", baseName);
            return Ok(new { status = "ok" });
        }

        [HttpGet("download/database/{baseName}")]
        public IActionResult DownloadDatabase([FromHeader] string token, string baseName)
        {
            var path = _service.GetDatabaseBackupPath(baseName);
            if (path == null)
                return NotFound();

            return PhysicalFile(path, "application/octet-stream", Path.GetFileName(path));
        }

        [HttpGet("download/media/{baseName}")]
        public IActionResult DownloadMedia([FromHeader] string token, string baseName)
        {
            var path = _service.GetMediaArchivePath(baseName);
            if (path == null)
                return NotFound();

            return PhysicalFile(path, "application/zip", Path.GetFileName(path));
        }
    }
}