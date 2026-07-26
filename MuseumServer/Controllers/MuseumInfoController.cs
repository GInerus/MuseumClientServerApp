using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MuseumServer.Attributes;
using MuseumServer.DTO;
using MuseumServer.Services;

namespace MuseumServer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [SessionAuthorize]
    public class MuseumInfoController : ControllerBase
    {
        private readonly MuseumInfoService _service;
        private readonly LoggingService _logging;

        public MuseumInfoController(MuseumInfoService service, LoggingService logging)
        {
            _service = service;
            _logging = logging;
        }

        // GET: api/MuseumInfo
        [HttpGet]
        public async Task<IActionResult> Get([FromHeader] string token)
        {
            var description = await _service.GetDescriptionAsync();
            return Ok(new { status = "ok", data = new { description } });
        }

        // PUT: api/MuseumInfo
        [HttpPut]
        [SessionAuthorize(adminOnly: true)]
        public async Task<IActionResult> Update(
            [FromHeader] string token,
            [FromBody] UpdateMuseumInfoRequest request)
        {
            var updated = await _service.UpdateDescriptionAsync(request.Description ?? string.Empty);
            return Ok(new { status = "ok", data = new { description = updated } });
        }

        [HttpPut("password")]
        [SessionAuthorize(adminOnly: true)]
        public async Task<IActionResult> ChangePassword([FromHeader] string token, [FromBody] ChangePasswordRequest request)
        {
            var result = await _service.ChangeAdminPasswordAsync(
                request.OldPassword,
                request.NewPassword);

            switch (result)
            {
                case ChangePasswordResult.Success:
                    await _logging.LogAsync("admin", "ChangePassword");

                    return Ok(new
                    {
                        status = "ok"
                    });


                case ChangePasswordResult.NewPasswordEmpty:

                    return BadRequest(new
                    {
                        status = "error",
                        message = "NEW_PASSWORD_EMPTY"
                    });


                case ChangePasswordResult.NewPasswordTooShort:

                    return BadRequest(new
                    {
                        status = "error",
                        message = "NEW_PASSWORD_TOO_SHORT"
                    });


                case ChangePasswordResult.OldPasswordInvalid:

                    return BadRequest(new
                    {
                        status = "error",
                        message = "OLD_PASSWORD_INVALID"
                    });


                default:

                    return BadRequest();
            }
        }
    }

    public class UpdateMuseumInfoRequest
    {
        public string? Description { get; set; }
    }
}