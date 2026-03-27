using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using MuseumServer.Services;


namespace MuseumServer.Attributes
{
    public class SessionAuthorizeAttribute : Attribute, IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var sessionService = context.HttpContext.RequestServices.GetService<SessionService>();

            var token = context.HttpContext.Request.Headers["token"].FirstOrDefault();

            if (string.IsNullOrEmpty(token) || sessionService == null || !sessionService.ValidateSession(token))
            {
                context.Result = new UnauthorizedObjectResult(new
                {
                    status = "error",
                    message = "Invalid or missing session token"
                });
                return;
            }

            await next();
        }
    }
}