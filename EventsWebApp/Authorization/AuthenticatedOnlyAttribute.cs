using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using EventsWebApp.Services.Interfaces;

namespace EventsWebApp.Authorization;

public class AuthenticatedOnlyAttribute : ActionFilterAttribute
{
    public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var userContextService = context.HttpContext.RequestServices.GetRequiredService<IUserContextService>();
        
        try
        {
            var isAuthenticated = await userContextService.IsAuthenticatedAsync();
            
            if (!isAuthenticated)
            {
                context.Result = new UnauthorizedResult();
                return;
            }
        }
        catch
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        await next();
    }
}