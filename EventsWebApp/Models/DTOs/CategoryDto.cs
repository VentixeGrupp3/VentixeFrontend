namespace EventsWebApp.Models.DTOs;

public class CategoryDto
{
    public string CategoryId { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public bool IsValid()
    {
        return !string.IsNullOrWhiteSpace(CategoryId) && !string.IsNullOrWhiteSpace(Name);
    }
}