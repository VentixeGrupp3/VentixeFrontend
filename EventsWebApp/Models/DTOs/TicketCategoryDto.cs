namespace EventsWebApp.Models.DTOs;

public class TicketCategoryDto
{
    public string TicketId { get; set; } = Guid.NewGuid().ToString();
    public string TicketCategory { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int AvailableQuantity { get; set; }
    public string Description { get; set; } = string.Empty;

    public bool IsPurchasable()
    {
        return !string.IsNullOrWhiteSpace(TicketCategory) && 
               AvailableQuantity != 0 && 
               Price >= 0;
    }

    public string GetApiFormattedPrice()
    {
        return Price.ToString("F2");
    }
}
