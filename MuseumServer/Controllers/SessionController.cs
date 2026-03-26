using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using MuseumServer.Models;
using MuseumServer.Services;

namespace MuseumServer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SessionController : ControllerBase
    {
        private readonly SessionService _sessionService;

        public SessionController(SessionService sessionService)
        {
            _sessionService = sessionService;
        }

        [HttpPost("register")]
        public IActionResult Register([FromBody] RegisterRequest req)
        {
            if (req.UserType == "admin" && !_sessionService.ValidateAdminPassword(req.Password))
                return BadRequest(new Response<object>("error", null, "ADMIN_AUTH_FAIL"));

            var token = _sessionService.CreateSession(req.UserType);
            return Ok(new Response<object>("ok", new { token }));
        }
    }
}