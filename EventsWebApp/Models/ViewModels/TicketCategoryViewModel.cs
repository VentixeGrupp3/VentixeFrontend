namespace EventsWebApp.ViewModels;

public class TicketCategoryViewModel
{
    public string TicketId { get; set; } = Guid.NewGuid().ToString();
    public string TicketCategory { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int AvailableQuantity { get; set; }
    public string Description { get; set; } = string.Empty;
    public string FormattedPrice
    {
        get
        {
            return Price == 0 ? "Free" : $"${Price:F2}";
        }
    }

    public string AvailabilityMessage
    {
        get
        {
            if (AvailableQuantity == 0)
                return "Sold Out";
            
            if (AvailableQuantity < 0)
                return "Available";
                
            if (AvailableQuantity <= 5)
                return $"Only {AvailableQuantity} left!";
                
            return $"{AvailableQuantity} available";
        }
    }
    public bool IsPurchasable => AvailableQuantity != 0 && Price >= 0;
}
