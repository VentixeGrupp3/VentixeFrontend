using Frontend_Test.Models;

namespace WebApp.Models.Booking;

public class BookingOrderViewModel
{
    public CreateBookingViewModel BookingInfo { get; set; } = new();
    public OrderTicketsViewModel TicketInfo { get; set; } = new();
}
