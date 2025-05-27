namespace WebApp.Models.DTOs;

public class UpdateTicketCategoryDto
{
    public decimal Price { get; set; }
    public int AvailableQuantity { get; set; }
    public string Description { get; set; } = string.Empty;
}