namespace Frontend_Test.Models;

public class BookingInfoViewModel
{
    public string UserId { get; set; } = null!;
    public string EventId { get; set; } = null!;
    public string TicketId { get; set; } = null!;
    public int TicketQuantity { get; set; }
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string StreetName { get; set; } = null!;
    public string PostalCode { get; set; } = null!;
    public string City { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Phone { get; set; } = null!;
}
