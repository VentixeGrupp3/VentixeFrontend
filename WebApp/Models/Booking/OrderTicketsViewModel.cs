namespace Frontend_Test.Models;

public class OrderTicketsViewModel
{
    public string TicketId { get; set; } = null!;
    public string TicketCategory { get; set; } = null!;
    public decimal TicketPrice { get; set; }
    public int TicketQuantity { get; set; }
}
