namespace WebApp.Models.Domain;

public class TicketCategory
{

    public string TicketId { get; set; } = Guid.NewGuid().ToString();

    public string Category { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public int AvailableQuantity { get; set; }

    public string Description { get; set; } = string.Empty;

    public int? MaxPerCustomer { get; set; }

    public bool IsAvailable { get; set; } = true;

    public DateTime? SaleStartDate { get; set; }

    public DateTime? SaleEndDate { get; set; }

    public bool IsCurrentlyAvailable
    {
        get
        {
            if (!IsAvailable || AvailableQuantity == 0)
                return false;

            var now = DateTime.Now;
            
            if (SaleStartDate.HasValue && now < SaleStartDate.Value)
                return false;

            if (SaleEndDate.HasValue && now > SaleEndDate.Value)
                return false;

            return true;
        }
    }

    public bool IsLimitedQuantity => AvailableQuantity > 0;


    public string GetFormattedPrice()
    {
        return Price == 0 ? "Free" : $"${Price:F2}";
    }

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

    public bool CanPurchase(int requestedQuantity)
    {
        if (!IsCurrentlyAvailable || requestedQuantity <= 0)
            return false;

        if (IsLimitedQuantity && requestedQuantity > AvailableQuantity)
            return false;

        if (MaxPerCustomer.HasValue && requestedQuantity > MaxPerCustomer.Value)
            return false;

        return true;
    }
}