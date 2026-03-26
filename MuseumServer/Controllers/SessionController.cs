using Microsoft.AspNetCore.Mvc;
using MuseumServer.Services;

namespace MuseumServe.Controllers
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
        public IActionResult Register([FromBody] SessionRequest request)
        {
            if (request.UserType == "admin" && !_sessionService.ValidateAdminPassword(request.Password))
            {
                return Unauthorized(new { status = "error", message = "ADMIN_AUTH_FAIL" });
            }

            string token = _sessionService.CreateSession(request.UserType);
            return Ok(new { status = "ok", token });
        }

        [HttpGet("validate/{token}")]
        public IActionResult Validate(string token)
        {
            bool valid = _sessionService.ValidateSession(token);
            if (!valid)
                return Unauthorized(new { status = "error", message = "INVALID_SESSION" });

            return Ok(new { status = "ok" });
        }
    }

    public class SessionRequest
    {
        public string UserType { get; set; } = "guest";
        public string Password { get; set; } = "";
    }
}