using Frontend_Test.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Frontend_Test.Controllers;

[Authorize(Roles = "User")]
public class UserBookingsController : Controller
{
    private readonly HttpClient _http;

    public UserBookingsController(HttpClient http)
    {
        _http = http;
        _http.BaseAddress = new Uri("https://aspnet2grupp3booking-epcudwa2fvd4cych.swedencentral-01.azurewebsites.net/api/Booking/");
    }

    private string GetCurrentUserId()
    => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    public async Task<IActionResult> Index()
    {

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Challenge();   // or BadRequest, RedirectToLogin, etc.

        // 2) Make sure we don’t double-add it if this controller is reused
        if (_http.DefaultRequestHeaders.Contains("x-user-id"))
            _http.DefaultRequestHeaders.Remove("x-user-id");

        // 3) Add it to your outgoing request
        _http.DefaultRequestHeaders.Add("x-user-id", userId);
        _http.DefaultRequestHeaders.Add("x-api-key", "1a76c263-4d83-4c98-b913-9029f9dfad7d");

        // 4) Fire off the call (your API “user-bookings” endpoint will now see the header)
        var bookings = await _http
            .GetFromJsonAsync<IEnumerable<BookingViewModel>>($"user-bookings/{userId}");

        return View(bookings);

        //    var bookings = await _http.GetFromJsonAsync<IEnumerable<BookingViewModel>>($"user-bookings/{userId}");

        //    return View(bookings);
    }

    public async Task<IActionResult> BookingDetails(string bookingId)
    {
        var booking = await _http.GetFromJsonAsync<BookingViewModel>($"bookingdetails/{bookingId}");

        var vm = new BookingViewModel
        {
            BookingId = booking.BookingId,
            BookingNumber = booking.BookingNumber,
            UserId = booking.UserId,
            BookingFirstName = booking.BookingFirstName,
            BookingLastName = booking.BookingLastName,
            BookingStreetName = booking.BookingStreetName,
            BookingCity = booking.BookingCity,
            BookingPostalCode = booking.BookingPostalCode,
            BookingEmail = booking.BookingEmail,
            BookingPhone = booking.BookingPhone,
            EventId = booking.EventId,
            EventName = booking.EventName,
            EventCategory = booking.EventCategory,
            EventDate = booking.EventDate,
            EventTime = booking.EventTime,
            Tickets = booking.Tickets
                          .Select(t => new TicketViewModel
                          {
                              TicketCategory = t.TicketCategory,
                              TicketPrice = t.TicketPrice,
                              TicketQuantity = t.TicketQuantity
                          })
                          .ToList()
        };

        return View(vm);
    }
}
