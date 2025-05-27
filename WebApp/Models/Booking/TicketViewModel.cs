namespace Frontend_Test.Models;

public class TicketViewModel
{
    public string TicketId { get; set; } = null!;

    public string BookingId { get; set; } = null!;

    public string TicketCategory { get; set; } = null!;
    public decimal TicketPrice { get; set; }
    public int TicketQuantity { get; set; }
}
