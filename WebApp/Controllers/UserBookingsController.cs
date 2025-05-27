using Frontend_Test.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;
using WebApp.Models.Booking;

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

    public async Task<IActionResult> Index()
    {

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Challenge();

        if (_http.DefaultRequestHeaders.Contains("x-user-id"))
            _http.DefaultRequestHeaders.Remove("x-user-id");

        _http.DefaultRequestHeaders.Add("x-user-id", userId);
        _http.DefaultRequestHeaders.Add("x-api-key", "1a76c263-4d83-4c98-b913-9029f9dfad7d");

        var bookings = await _http
            .GetFromJsonAsync<IEnumerable<BookingViewModel>>($"user-bookings/{userId}");

        return View(bookings);
    }

    public async Task<IActionResult> BookingDetails(string bookingId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Challenge();

        if (_http.DefaultRequestHeaders.Contains("x-user-id"))
            _http.DefaultRequestHeaders.Remove("x-user-id");

        _http.DefaultRequestHeaders.Add("x-user-id", bookingId);
        _http.DefaultRequestHeaders.Add("x-api-key", "1a76c263-4d83-4c98-b913-9029f9dfad7d");

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

    public IActionResult ReserveTickets(string id)
    {
        decimal regularPrice = 100m;
        decimal vipPrice = 250m;

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var vm = new BookingOrderViewModel
        {
            BookingInfo = new BookingInfoViewModel
            {
                EventId = id,
                UserId = userId
            },
            TicketInfo = new OrderTicketsViewModel
            {
                CategoryList = new[] {
                new SelectListItem($"Regular – {regularPrice:C}", "Regular"),
                new SelectListItem($"VIP – {vipPrice:C}",     "VIP")
            },
                QuantityList = Enumerable.Range(1, 9)
                                     .Select(i => new SelectListItem(i.ToString(), i.ToString())),
                TicketCategory = "Regular",
                TicketPrice = regularPrice,
                TicketQuantity = 1
            }
        };

        return View(vm);
    }

    [HttpPost]
    public async Task<IActionResult> OrderTickets(BookingOrderViewModel formData)
    {
        var response = await _http.PostAsJsonAsync("user-create", formData);

        if (response.IsSuccessStatusCode)
            return RedirectToAction("Index", "Bookings");

        return View();
    }
}
