namespace WebApp.Models.DTOs;

public class CreateTicketCategoryDto
{
    public string TicketCategory { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int AvailableQuantity { get; set; }
    public string Description { get; set; } = string.Empty;
}
