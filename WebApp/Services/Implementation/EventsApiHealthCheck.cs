using Microsoft.Extensions.Diagnostics.HealthChecks;
using WebApp.Services.Interfaces;

namespace WebApp.Services.Implementation;
//Compiled By AI
public class EventsApiHealthCheck(
    IEventsApiService eventsApiService,
    ILogger<EventsApiHealthCheck> logger,
    IConfigurationService configurationService) : IHealthCheck
{
    private readonly IEventsApiService _eventsApiService = eventsApiService ?? throw new ArgumentNullException(nameof(eventsApiService));
    private readonly ILogger<EventsApiHealthCheck> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IConfigurationService _configurationService = configurationService ?? throw new ArgumentNullException(nameof(configurationService));

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Starting health check for Events API");
            
            var isHealthy = await _eventsApiService.IsApiHealthyAsync();
            
            if (isHealthy)
            {
                var apiUrl = _configurationService.GetEventsApiBaseUrl();
                _logger.LogDebug("Events API health check passed for {ApiUrl}", apiUrl);
                
                return HealthCheckResult.Healthy($"Events API at {apiUrl} is responding correctly");
            }
            else
            {
                _logger.LogWarning("Events API health check failed - API not responding");
                return HealthCheckResult.Unhealthy("Events API is not responding to health check requests");
            }
        }
        catch (HttpRequestException httpEx)
        {
            _logger.LogError(httpEx, "Network error during Events API health check");
            return HealthCheckResult.Unhealthy($"Network connectivity issue with Events API: {httpEx.Message}");
        }
        catch (TaskCanceledException timeoutEx)
        {
            _logger.LogError(timeoutEx, "Timeout during Events API health check");
            return HealthCheckResult.Unhealthy("Events API health check timed out - API may be slow or unavailable");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during Events API health check");
            return HealthCheckResult.Unhealthy($"Unexpected error during API health check: {ex.Message}");
        }
    }
}