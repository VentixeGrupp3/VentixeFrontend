namespace EventsWebApp.Models.DTOs;

public class CategoryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ColorCode { get; set; } = string.Empty;    
    public int EventCount { get; set; }         

    public bool IsValid()
    {
        return Id > 0 && !string.IsNullOrWhiteSpace(Name);
    }
}