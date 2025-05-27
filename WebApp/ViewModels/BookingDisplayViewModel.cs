namespace WebApp.ViewModels;
public class BookingDisplayViewModel
{
    public string BookingId { get; set; } = string.Empty;
    public string EventId { get; set; } = string.Empty;
    public string EventName { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public string TicketType { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public DateTime BookingDate { get; set; }
    public string Status { get; set; } = string.Empty;
    
    public string FormattedBookingDate => BookingDate.ToString("MMM dd, yyyy 'at' h:mm tt");
}