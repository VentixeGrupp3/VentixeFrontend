using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace WebApp.Authentication;

public class ApiKeyAuthenticationSchemeOptions : AuthenticationSchemeOptions
{
    public const string DefaultScheme = "ApiKey";
}

public class ApiKeyAuthenticationHandler(
    IOptionsMonitor<ApiKeyAuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder,
    IConfiguration configuration) : AuthenticationHandler<ApiKeyAuthenticationSchemeOptions>(options, logger, encoder)
{
    private readonly IConfiguration _configuration = configuration;
    private readonly ILogger<ApiKeyAuthenticationHandler> _logger = logger.CreateLogger<ApiKeyAuthenticationHandler>();

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        try
        {
            // Get API key from header
            if (!Request.Headers.TryGetValue("x-api-key", out var apiKeyHeaderValues))
            {
                // No API key provided - allow anonymous access
                return Task.FromResult(AuthenticateResult.NoResult());
            }

            var providedApiKey = apiKeyHeaderValues.FirstOrDefault();
            if (string.IsNullOrWhiteSpace(providedApiKey))
            {
                return Task.FromResult(AuthenticateResult.NoResult());
            }

            var adminApiKey = _configuration["Services:EventsApi:AdminApiKey"];
            var userApiKey = _configuration["Services:EventsApi:UserApiKey"];

            string role;
            string userName;

            // Determine role based on which key was provided
            if (!string.IsNullOrEmpty(adminApiKey) && providedApiKey == adminApiKey)
            {
                role = "Admin";
                userName = "Admin User";
                _logger.LogInformation("Admin authenticated with API key");
            }
            else if (!string.IsNullOrEmpty(userApiKey) && providedApiKey == userApiKey)
            {
                role = "User";
                userName = "Standard User";
                _logger.LogInformation("User authenticated with API key");
            }
            else
            {
                _logger.LogWarning("Invalid API key provided: {ApiKey}", providedApiKey?.Substring(0, Math.Min(8, providedApiKey.Length)) + "...");
                return Task.FromResult(AuthenticateResult.Fail("Invalid API key"));
            }

            var claims = new[]
            {
                new Claim(ClaimTypes.Name, userName),
                new Claim(ClaimTypes.Role, role),
                new Claim("ApiKey", providedApiKey)
            };

            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);

            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during API key authentication");
            return Task.FromResult(AuthenticateResult.Fail("Authentication error"));
        }
    }
}