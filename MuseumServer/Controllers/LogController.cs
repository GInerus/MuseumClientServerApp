using Microsoft.AspNetCore.Mvc;
using MuseumServer.Attributes;
using MuseumServer.Services;

namespace MuseumServer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [SessionAuthorize(adminOnly: true)]
    public class LogController : ControllerBase
    {
        private readonly LoggingService _logging;

        public LogController(LoggingService logging)
        {
            _logging = logging;
        }

        // GET: api/Log
        [HttpGet]
        public async Task<IActionResult> GetAll([FromHeader] string token)
        {
            var logs = await _logging.GetAllAsync();
            return Ok(new { status = "ok", data = logs });
        }
    }
}