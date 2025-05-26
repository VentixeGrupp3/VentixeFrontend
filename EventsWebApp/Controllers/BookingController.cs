// EventsWebApp/Controllers/BookingController.cs
using EventsWebApp.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventsWebApp.Controllers;

[Authorize(Roles = "User,Admin")]
public class BookingController(ILogger<BookingController> logger) : Controller
{
    private readonly ILogger<BookingController> _logger = logger;

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Reserve(ReservationViewModel model)
    {
        _logger.LogInformation("Processing reservation for event {EventId} by {CustomerName}", 
                              model.EventId, model.CustomerName);

        var errors = new List<string>();
        
        if (string.IsNullOrWhiteSpace(model.EventId))
            errors.Add("Event information is missing.");
        if (string.IsNullOrWhiteSpace(model.CustomerName))
            errors.Add("Full name is required.");
        if (string.IsNullOrWhiteSpace(model.CustomerEmail))
            errors.Add("Email is required.");
        else if (!model.CustomerEmail.Contains("@"))
            errors.Add("Please enter a valid email address.");
        if (string.IsNullOrWhiteSpace(model.CustomerPhone))
            errors.Add("Phone number is required.");
        if (string.IsNullOrWhiteSpace(model.TicketType))
            errors.Add("Please select a ticket type.");
        if (model.Quantity < 1 || model.Quantity > 10)
            errors.Add("Quantity must be between 1 and 10.");

        if (errors.Any())
        {
            _logger.LogWarning("Validation failed: {Errors}", string.Join(", ", errors));
            TempData["Error"] = string.Join(" ", errors);
            return RedirectToAction("Details", "Events", new { id = model.EventId });
        }

        try
        {
            _logger.LogInformation("Reservation processed successfully for event {EventId}", model.EventId);
            TempData["ReservationData"] = System.Text.Json.JsonSerializer.Serialize(model);
            TempData["Success"] = "Reservation submitted successfully!";
            return RedirectToAction("Index");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing reservation for event {EventId}", model.EventId);
            TempData["Error"] = "Unable to process reservation. Please try again.";
            return RedirectToAction("Details", "Events", new { id = model.EventId });
        }
    }

    public IActionResult Index()
    {
        _logger.LogInformation("Loading booking index page");
        var bookings = new List<BookingDisplayViewModel>();

        if (TempData["ReservationData"] is string reservationJson)
        {
            try
            {
                var reservation = System.Text.Json.JsonSerializer.Deserialize<ReservationViewModel>(reservationJson);
                if (reservation != null)
                {
                    bookings.Add(new BookingDisplayViewModel
                    {
                        BookingId = Guid.NewGuid().ToString(),
                        EventId = reservation.EventId,
                        EventName = reservation.EventName,
                        CustomerName = reservation.CustomerName,
                        CustomerEmail = reservation.CustomerEmail,
                        CustomerPhone = reservation.CustomerPhone,
                        TicketType = reservation.TicketType,
                        Quantity = reservation.Quantity,
                        BookingDate = DateTime.Now,
                        Status = "Reserved"
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deserializing reservation data");
            }
        }

        return View(bookings);
    }
}