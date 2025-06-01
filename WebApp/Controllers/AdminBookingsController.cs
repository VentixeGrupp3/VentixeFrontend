using Frontend_Test.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.SqlServer.Server;
using System.Security.Claims;

namespace WebApp.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminBookingsController : Controller
    {
        private readonly HttpClient _http;


        public AdminBookingsController(HttpClient http)
        {
            _http = http;
            _http.BaseAddress = new Uri("https://aspnet2grupp3booking-epcudwa2fvd4cych.swedencentral-01.azurewebsites.net/api/Booking/");
        }

        public async Task<IActionResult> Index()
        {


            _http.DefaultRequestHeaders.Add("x-api-key", "fba16aa0-4bb4-4bb7-9201-d81937292329");

            var bookings = await _http
                .GetFromJsonAsync<IEnumerable<BookingViewModel>>($"admin-get-all");

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
            _http.DefaultRequestHeaders.Add("x-api-key", "fba16aa0-4bb4-4bb7-9201-d81937292329");

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


        public IActionResult ReserveTickets(string eventId)
        {

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var vm = new BookingInfoViewModel
            {
                EventId = eventId,
                UserId = userId
            };

            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> OrderTickets(BookingInfoViewModel formData)
        {
            _http.DefaultRequestHeaders.Add("x-api-key", "fba16aa0-4bb4-4bb7-9201-d81937292329");

            var response = await _http.PostAsJsonAsync("admin-create", formData);

            if (response.IsSuccessStatusCode)
                return RedirectToAction("Index", "Bookings");

            return View();
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteBooking(string bookingId)
        {
            _http.DefaultRequestHeaders.Add("x-api-key", "fba16aa0-4bb4-4bb7-9201-d81937292329");

            var response = await _http.PostAsJsonAsync("admin-delete/", bookingId);

            if (response.IsSuccessStatusCode)
                return RedirectToAction("Index", "Bookings");

            return View();
        }
    }
}
