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
    }
}