namespace EventsWebApp.Configuration;

public class ApiConfiguration
{
    public const string SectionName = "Services:EventsApi";

    public string BaseUrl { get; set; } = string.Empty;
    public string AdminApiKey { get; set; } = string.Empty;
    public int TimeoutSeconds { get; set; } = 30;
}
