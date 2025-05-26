using EventsWebApp.Services.Interfaces;
using System.Security.Claims;

namespace EventsWebApp.Services.Implementation;

public class UserContextService : IUserContextService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<UserContextService> _logger;

    public UserContextService(
        IHttpContextAccessor httpContextAccessor,
        ILogger<UserContextService> logger)
    {
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    public async Task<string> GetCurrentUserNameAsync()
    {
        await Task.CompletedTask;
        
        try
        {
            var user = _httpContextAccessor.HttpContext?.User;
            if (user?.Identity?.IsAuthenticated == true)
            {
                var name = user.FindFirst(ClaimTypes.Name)?.Value;
                return !string.IsNullOrEmpty(name) ? name : "Authenticated User";
            }
            return "Guest User";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting current user name");
            return "Guest User";
        }
    }

    public async Task<string> GetCurrentUserRoleAsync()
    {
        await Task.CompletedTask;
        
        try
        {
            var user = _httpContextAccessor.HttpContext?.User;
            if (user?.Identity?.IsAuthenticated == true)
            {
                var role = user.FindFirst(ClaimTypes.Role)?.Value;
                return !string.IsNullOrEmpty(role) ? role : "User";
            }
            return "Guest";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting current user role");
            return "Guest";
        }
    }

    public async Task<bool> IsAdminAsync()
    {
        await Task.CompletedTask;
        
        try
        {
            var user = _httpContextAccessor.HttpContext?.User;
            return user?.IsInRole("Admin") == true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking admin status");
            return false;
        }
    }

    public async Task<bool> IsAuthenticatedAsync()
    {
        await Task.CompletedTask;
        
        try
        {
            var user = _httpContextAccessor.HttpContext?.User;
            return user?.Identity?.IsAuthenticated == true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking authentication status");
            return false;
        }
    }
}