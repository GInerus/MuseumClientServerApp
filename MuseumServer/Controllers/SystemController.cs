using Microsoft.AspNetCore.Mvc;
using System.Reflection;

namespace MuseumServer.Controllers
{
    [ApiController]
    [Route("api/system")]
    public class SystemController : ControllerBase
    {
        // GET: api/system/version
        [HttpGet("version")]
        public IActionResult GetVersion()
        {
            var version = Assembly.GetExecutingAssembly().GetName().Version;

            return Ok(new
            {
                status = "ok",
                data = new { version = version?.ToString() ?? "неизвестно" }
            });
        }

        // GET: api/system/guide
        [HttpGet("guide")]
        public IActionResult GetGuide()
        {
            var path = Path.Combine(
                Directory.GetCurrentDirectory(),
                "wwwroot",
                "system",
                "UserGuide.pdf");


            if (!System.IO.File.Exists(path))
            {
                return NotFound(new
                {
                    status = "error",
                    message = "User guide not found"
                });
            }


            return PhysicalFile(
                path,
                "application/pdf",
                enableRangeProcessing: true);
        }
    }
}