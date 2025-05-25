namespace EventsWebApp.Models.ViewModels;

public class ReservationViewModel
{
    public string EventId { get; set; } = string.Empty;
    public string EventName { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public string TicketType { get; set; } = string.Empty;
    public int Quantity { get; set; } = 1;
}

