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

    /// <summary>
    /// Helper method to get display-friendly category name.
    /// Falls back to ID if name is missing (shouldn't happen in normal operation).
    /// </summary>
    public string GetDisplayName()
    {
        return string.IsNullOrWhiteSpace(Name) ? CategoryId : Name;
    }
}