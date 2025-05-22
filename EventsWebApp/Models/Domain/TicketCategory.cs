namespace EventsWebApp.Models.Domain;

public class TicketCategory
{

    public string TicketId { get; set; } = Guid.NewGuid().ToString();

    public string TicketCategory { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public int AvailableQuantity { get; set; }

    public string Description { get; set; } = string.Empty;

    public int? MaxPerCustomer { get; set; }

    public bool IsAvailable { get; set; } = true;

    public DateTime? SaleStartDate { get; set; }

    public DateTime? SaleEndDate { get; set; }

    /// <summary>
    /// Helper property to determine if tickets are currently on sale.
    /// Considers availability, sale dates, and quantity in stock.
    /// </summary>
    public bool IsCurrentlyAvailable
    {
        get
        {
            if (!IsAvailable || AvailableQuantity == 0)
                return false;

            var now = DateTime.Now;
            
            // Check if sale period has started
            if (SaleStartDate.HasValue && now < SaleStartDate.Value)
                return false;

            // Check if sale period has ended
            if (SaleEndDate.HasValue && now > SaleEndDate.Value)
                return false;

            return true;
        }
    }

    /// <summary>
    /// Helper property to check if this is a limited quantity ticket.
    /// Returns true if there's a specific number available (not unlimited).
    /// </summary>
    public bool IsLimitedQuantity => AvailableQuantity > 0;

    /// <summary>
    /// Helper method to format price for display.
    /// Handles currency formatting in a consistent way.
    /// </summary>
    public string GetFormattedPrice()
    {
        return Price == 0 ? "Free" : $"${Price:F2}";
    }

    /// <summary>
    /// Helper method to get availability status message.
    /// Provides user-friendly text about ticket availability.
    /// </summary>
    public string GetAvailabilityMessage()
    {
        if (!IsCurrentlyAvailable)
        {
            if (AvailableQuantity == 0)
                return "Sold Out";
            
            if (SaleStartDate.HasValue && DateTime.Now < SaleStartDate.Value)
                return $"Sale starts {SaleStartDate.Value:MMM dd, yyyy}";
            
            if (SaleEndDate.HasValue && DateTime.Now > SaleEndDate.Value)
                return "Sale ended";
                
            return "Not available";
        }

        if (AvailableQuantity < 0)
            return "Available";

        if (AvailableQuantity <= 10)
            return $"Only {AvailableQuantity} left!";

        return $"{AvailableQuantity} available";
    }

    /// <summary>
    /// Helper method to check if a customer can purchase a specific quantity.
    /// Enforces both availability and per-customer limits.
    /// </summary>
    public bool CanPurchase(int requestedQuantity)
    {
        if (!IsCurrentlyAvailable || requestedQuantity <= 0)
            return false;

        // Check if enough tickets are available
        if (IsLimitedQuantity && requestedQuantity > AvailableQuantity)
            return false;

        // Check per-customer limit
        if (MaxPerCustomer.HasValue && requestedQuantity > MaxPerCustomer.Value)
            return false;

        return true;
    }
}