using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using MuseumServer.Services;

namespace MuseumServer.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class SessionAuthorizeAttribute : Attribute, IAsyncActionFilter
    {
        private readonly bool _adminOnly;

        // adminOnly = true → только админ может выполнять
        public SessionAuthorizeAttribute(bool adminOnly = false)
        {
            _adminOnly = adminOnly;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var sessionService = (SessionService)context.HttpContext.RequestServices.GetService(typeof(SessionService))!;
            var token = context.HttpContext.Request.Headers["token"].FirstOrDefault();

            if (string.IsNullOrEmpty(token) || sessionService == null)
            {
                context.Result = new JsonResult(new { status = "error", message = "Missing session token" }) { StatusCode = 401 };
                return;
            }

            var session = sessionService.GetSession(token);
            if (session == null)
            {
                context.Result = new JsonResult(new { status = "error", message = "Invalid or expired session token" }) { StatusCode = 401 };
                return;
            }

            // Проверяем, что действие разрешено только для Admin
            if (_adminOnly && session.UserType != "admin")
            {
                context.Result = new JsonResult(new { status = "error", message = "Insufficient permissions" }) { StatusCode = 403 };
                return;
            }

            await next();
        }
    }
}