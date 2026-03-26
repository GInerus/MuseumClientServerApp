using Microsoft.AspNetCore.Identity.Data;
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
            var allowedUserTypes = new[] { "guest", "admin" };

            if (string.IsNullOrWhiteSpace(request.UserType) || !allowedUserTypes.Contains(request.UserType))
                return BadRequest(new { status = "error", message = "INVALID_USER_TYPE" });

            if (request.UserType == "admin" && !_sessionService.ValidateAdminPassword(request.Password))
                return Unauthorized(new { status = "error", message = "ADMIN_AUTH_FAIL" });

            var token = _sessionService.CreateSession(request.UserType);

            return Ok(new { status = "ok", token, userType = request.UserType });
        }

        [HttpGet("validate/{token}")]
        public IActionResult Validate(string token)
        {
            var session = _sessionService.GetSession(token);

            if (session == null)
                return Unauthorized(new { status = "error", message = "INVALID_SESSION" });

            return Ok(new { status = "ok", userType = session.UserType });
        }
    }

    public class SessionRequest
    {
        public string UserType { get; set; } = "guest";
        public string Password { get; set; } = "";
    }
}