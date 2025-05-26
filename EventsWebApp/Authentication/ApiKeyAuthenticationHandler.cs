using EventsWebApp.Services.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace EventsWebApp.Authentication;

public class ApiKeyAuthenticationHandler : AuthenticationHandler<ApiKeyAuthenticationSchemeOptions>
{
    private readonly IEventsApiService _eventsApiService;
    private readonly ILogger<ApiKeyAuthenticationHandler> _logger;

    public ApiKeyAuthenticationHandler(
        IOptionsMonitor<ApiKeyAuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        ISystemClock clock,
        IEventsApiService eventsApiService)
        : base(options, logger, encoder, clock)
    {
        _eventsApiService = eventsApiService;
        _logger = logger.CreateLogger<ApiKeyAuthenticationHandler>();
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        try
        {
            // Check if user has admin access via API key
            var isAdmin = await _eventsApiService.IsAdminAsync();
            
            var claims = new List<Claim>
            {
                new(ClaimTypes.Name, isAdmin ? "Event Administrator" : "Event User"),
                new(ClaimTypes.Role, isAdmin ? "Admin" : "User"),
                new("ApiKeyAccess", "true")
            };

            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);

            _logger.LogDebug("Authentication successful. Role: {Role}", isAdmin ? "Admin" : "User");
            
            return AuthenticateResult.Success(ticket);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Authentication failed");
            
            // Create a guest user for failed API key authentication
            var guestClaims = new List<Claim>
            {
                new(ClaimTypes.Name, "Guest User"),
                new(ClaimTypes.Role, "Guest"),
                new("ApiKeyAccess", "false")
            };

            var guestIdentity = new ClaimsIdentity(guestClaims, Scheme.Name);
            var guestPrincipal = new ClaimsPrincipal(guestIdentity);
            var guestTicket = new AuthenticationTicket(guestPrincipal, Scheme.Name);

            return AuthenticateResult.Success(guestTicket);
        }
    }
}
