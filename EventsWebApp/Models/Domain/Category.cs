namespace EventsWebApp.Models.Domain;

public class Category
{
    public string CategoryId { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string? Color { get; set; }

    public bool IsActive { get; set; } = true;


    public bool CanCreateEvents()
    {
        return IsActive && !string.IsNullOrWhiteSpace(Name);
    }
    public string GetDisplayName()
    {
        return string.IsNullOrWhiteSpace(Name) ? CategoryId : Name;
    }
}