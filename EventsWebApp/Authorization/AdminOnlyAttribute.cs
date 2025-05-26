using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using EventsWebApp.Services.Interfaces;

namespace EventsWebApp.Authorization;

public class AdminOnlyAttribute : ActionFilterAttribute
{
    public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var userRoleService = context.HttpContext.RequestServices.GetRequiredService<IUserRoleService>();
        
        try
        {
            var isAdmin = await userRoleService.IsAdminAsync();
            
            if (!isAdmin)
            {
                context.Result = new ForbidResult();
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