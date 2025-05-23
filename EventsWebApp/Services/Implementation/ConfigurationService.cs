using EventsWebApp.Configuration;
using EventsWebApp.Services.Interfaces;
using Microsoft.Extensions.Options;

namespace EventsWebApp.Services.Implementation;

public class ConfigurationService(
    IOptions<ApiConfiguration> apiConfig,
    IOptions<ApplicationConfiguration> appConfig,
    ILogger<ConfigurationService> logger) : IConfigurationService
{
    private readonly ApiConfiguration _apiConfig = apiConfig?.Value ?? throw new ArgumentNullException(nameof(apiConfig));
    private readonly ApplicationConfiguration _appConfig = appConfig?.Value ?? throw new ArgumentNullException(nameof(appConfig));
    private readonly ILogger<ConfigurationService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public string GetEventsApiBaseUrl()
    {
        var baseUrl = _apiConfig.BaseUrl;
        
        
        if (IsDevelopment() && string.IsNullOrWhiteSpace(baseUrl))
        {
            _logger.LogWarning("No API base URL configured, using development default");
            return "https://localhost:7103";
        }

        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            throw new InvalidOperationException("Events API base URL is not configured");
        }

        return baseUrl;
    }

    public string GetEventsApiKey()
    {
        var apiKey = Environment.GetEnvironmentVariable("EVENTS_API_KEY");
        
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            apiKey = _apiConfig.AdminApiKey;
            
            if (!IsDevelopment() && !string.IsNullOrWhiteSpace(apiKey))
            {
                _logger.LogWarning("API key loaded from configuration file in production environment");
            }
        }

        if (string.IsNullOrWhiteSpace(apiKey) && IsDevelopment())
        {
            _logger.LogWarning("No API key configured - API calls may fail");
            return string.Empty;
        }

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new InvalidOperationException("Events API key is not configured");
        }

        return apiKey;
    }
    public TimeSpan GetApiTimeout()
    {
        var timeoutSeconds = _apiConfig.TimeoutSeconds;
        
        if (timeoutSeconds < 5)
        {
            _logger.LogWarning("API timeout too low ({Seconds}s), using minimum of 5 seconds", timeoutSeconds);
            timeoutSeconds = 5;
        }
        else if (timeoutSeconds > 300)
        {
            _logger.LogWarning("API timeout too high ({Seconds}s), using maximum of 300 seconds", timeoutSeconds);
            timeoutSeconds = 300;
        }

        return TimeSpan.FromSeconds(timeoutSeconds);
    }
    public bool ValidateConfiguration()
    {
        var isValid = true;
        var validationErrors = new List<string>();

        try
        {
            var baseUrl = GetEventsApiBaseUrl();
            if (!Uri.TryCreate(baseUrl, UriKind.Absolute, out _))
            {
                validationErrors.Add($"Invalid API base URL: {baseUrl}");
                isValid = false;
            }
        }
        catch (Exception ex)
        {
            validationErrors.Add($"API base URL validation failed: {ex.Message}");
            isValid = false;
        }

        if (!IsDevelopment())
        {
            try
            {
                var apiKey = GetEventsApiKey();
                if (string.IsNullOrWhiteSpace(apiKey))
                {
                    validationErrors.Add("API key is required in production");
                    isValid = false;
                }
            }
            catch (Exception ex)
            {
                validationErrors.Add($"API key validation failed: {ex.Message}");
                isValid = false;
            }
        }

        if (isValid)
        {
            _logger.LogInformation("Configuration validation passed");
        }
        else
        {
            _logger.LogError("Configuration validation failed: {Errors}", string.Join(", ", validationErrors));
        }

        return isValid;
    }

    public string GetEnvironmentName()
    {
        return _appConfig.Environment;
    }
    public bool IsDevelopment()
    {
        return string.Equals(_appConfig.Environment, "Development", StringComparison.OrdinalIgnoreCase);
    }
}