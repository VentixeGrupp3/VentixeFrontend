namespace EventsWebApp.Services.Interfaces;

public interface IUserContextService
{
    Task<string> GetCurrentUserNameAsync();
    Task<string> GetCurrentUserRoleAsync();
    Task<bool> IsAdminAsync();
    Task<bool> IsAuthenticatedAsync();
}