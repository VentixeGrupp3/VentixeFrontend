namespace WebApp.Configuration;

public class ApplicationConfiguration
{
    public const string SectionName = "Application";

    public string Environment { get; set; } = "Production";
    public bool EnableDetailedErrors { get; set; } = false;
    public string LogLevel { get; set; } = "Information";
}
