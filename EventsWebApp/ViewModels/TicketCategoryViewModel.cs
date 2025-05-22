namespace EventsWebApp.ViewModels;

public class TicketCategoryViewModel
{
    public string TicketId { get; set; } = Guid.NewGuid().ToString();
    public string TicketCategories { get; set; }
    public decimal Price { get; set; }
    public int AvailableQuantity { get; set; }
}