namespace EventsWebApp.Services.Interfaces;

public interface IConfigurationService
{
    string GetEventsApiBaseUrl();
    string GetEventsApiKey();
    TimeSpan GetApiTimeout();
    bool ValidateConfiguration();
    string GetEnvironmentName();
    bool IsDevelopment();
}
