namespace WebApp.ViewModels;

public class BookingRedirectViewModel
{
    // Event Data (matches proto structure)
    public string EventId { get; set; } = string.Empty;
    public string EventName { get; set; } = string.Empty;
    public string EventCategory { get; set; } = string.Empty;
    public string EventDate { get; set; } = string.Empty;
    public string EventTime { get; set; } = string.Empty;
    public string EventLocation { get; set; } = string.Empty;
    public string VenueName { get; set; } = string.Empty;

    // Event Owner Data (matches proto structure)
    public string EventOwnerName { get; set; } = string.Empty;
    public string EventOwnerEmail { get; set; } = string.Empty;
    public string EventOwnerAddress { get; set; } = string.Empty;
    public string EventOwnerPhone { get; set; } = string.Empty;

    // Customer Data
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;

    // Ticket Selection (matches proto ticket structure)
    public string SelectedTicketCategory { get; set; } = string.Empty;
    public double SelectedTicketPrice { get; set; }
    public int TicketQuantity { get; set; } = 1;
}
