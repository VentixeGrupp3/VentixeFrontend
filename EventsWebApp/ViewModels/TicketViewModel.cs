namespace EventsWebApp.ViewModels;

public class TicketViewModel
{
    public string TicketId { get; set; } = Guid.NewGuid().ToString();
    public string CategoryName { get; set; }
    public decimal Price { get; set; }
    public int AvailableQuantity { get; set; }
    public string Description { get; set; }
}
