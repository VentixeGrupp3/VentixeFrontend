using EventsWebApp.Services.Interfaces;

public interface IUserRoleService
{
    Task<string> GetCurrentUserRoleAsync();
    Task<bool> IsAdminAsync();
    Task<bool> CanCreateEventsAsync();
    Task<bool> CanEditEventsAsync();
    Task<bool> CanDeleteEventsAsync();
}

public class UserRoleService(IEventsApiService eventsApiService, ILogger<UserRoleService> logger) : IUserRoleService
{
    private readonly IEventsApiService _eventsApiService = eventsApiService;
    private readonly ILogger<UserRoleService> _logger = logger;

    public async Task<string> GetCurrentUserRoleAsync()
    {
        try
        {
            var canAccessAdmin = await _eventsApiService.IsAdminAsync();
            return canAccessAdmin ? "Admin" : "User";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking user role");
            return "User"; // Default to user if can't determine
        }
    }

    public async Task<bool> IsAdminAsync()
    {
        var role = await GetCurrentUserRoleAsync();
        return role == "Admin";
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