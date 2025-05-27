using WebApp.Services.Interfaces;
using System.Security.Claims;

public interface IUserRoleService
{
    Task<string> GetCurrentUserRoleAsync();
    Task<bool> IsAdminAsync();
    Task<bool> CanCreateEventsAsync();
    Task<bool> CanEditEventsAsync();
    Task<bool> CanDeleteEventsAsync();
}

public class UserRoleService(
    IHttpContextAccessor httpContextAccessor,
    ILogger<UserRoleService> logger) : IUserRoleService
{
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
    private readonly ILogger<UserRoleService> _logger = logger;

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
            _logger.LogError(ex, "Error checking user role");
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

    public async Task<bool> CanCreateEventsAsync()
    {
        return await IsAdminAsync();
    }

    public async Task<bool> CanEditEventsAsync()
    {
        return await IsAdminAsync();
    }

    public async Task<bool> CanDeleteEventsAsync()
    {
        return await IsAdminAsync();
    }
}