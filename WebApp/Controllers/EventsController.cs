using WebApp.Models.Domain;
using WebApp.Services.Interfaces;
using WebApp.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace WebApp.Controllers;

[Authorize] // All actions require authentication
public class EventsController(
    IEventsApiService eventsApiService,
    IModelMappingService mappingService,
    ILogger<EventsController> logger) : Controller
{
    private readonly IEventsApiService _eventsApiService = eventsApiService;
    private readonly IModelMappingService _mappingService = mappingService;
    private readonly ILogger<EventsController> _logger = logger;

    [AllowAnonymous]
    public async Task<IActionResult> Index()
    {
        _logger.LogInformation("Loading events index page");
        try
        {
            var events = await _eventsApiService.GetAllEventsAsync();
            var categories = await _eventsApiService.GetAllCategoriesAsync();
            
            var eventViewModels = events.Select(e => _mappingService.MapToEventCardViewModel(e)).ToList();
            ViewBag.Categories = categories.ToList();

            _logger.LogInformation("Successfully loaded {EventCount} events and {CategoryCount} categories", 
                                  events.Count(), categories.Count());
            return View(eventViewModels);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading events index page");
            throw;
        }
    }

    [AllowAnonymous]
    public async Task<IActionResult> Details(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            _logger.LogWarning("Details action called with null or empty id");
            return NotFound();
        }

        _logger.LogInformation("Loading details for event {EventId}", id);
        try
        {
            var eventModel = await _eventsApiService.GetEventByIdAsync(id);
            if (eventModel == null)
            {
                _logger.LogInformation("Event {EventId} not found", id);
                return NotFound();
            }

            eventModel.TicketCategories = GenerateMockTickets(eventModel.CategoryId);
            var viewModel = _mappingService.MapToEventFormViewModel(eventModel);

            _logger.LogInformation("Successfully loaded details for event {EventId}: {EventName}", 
                                  id, eventModel.EventName);
            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading details for event {EventId}", id);
            throw;
        }
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create()
    {
        _logger.LogInformation("Loading create event form");
        try
        {
            await PopulateCategoriesViewBag();
            return View(new EventFormViewModel());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading create event form");
            throw;
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create(EventFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            _logger.LogInformation("Create event form validation failed");
            await PopulateCategoriesViewBag();
            return View(model);
        }

        _logger.LogInformation("Creating new event: {EventName}", model.EventName);
        try
        {
            var eventModel = ConvertFormViewModelToEvent(model);
            var success = await _eventsApiService.CreateEventAsync(eventModel);

            if (success)
            {
                _logger.LogInformation("Successfully created event: {EventName}", model.EventName);
                TempData["Success"] = "Event created successfully!";
                return RedirectToAction(nameof(Index));
            }
            else
            {
                _logger.LogWarning("Failed to create event: {EventName}", model.EventName);
                ModelState.AddModelError(string.Empty, "Unable to create event. Please try again.");
                await PopulateCategoriesViewBag();
                return View(model);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating event: {EventName}", model.EventName);
            throw;
        }
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Edit(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            return NotFound();

        _logger.LogInformation("Loading edit form for event {EventId}", id);
        try
        {
            var eventModel = await _eventsApiService.GetEventByIdAsync(id);
            if (eventModel == null)
                return NotFound();

            var viewModel = _mappingService.MapToEventFormViewModel(eventModel);
            await PopulateCategoriesViewBag();
            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading edit form for event {EventId}", id);
            throw;
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Edit(string id, EventFormViewModel model)
    {
        if (string.IsNullOrWhiteSpace(id))
            return NotFound();

        if (!ModelState.IsValid)
        {
            await PopulateCategoriesViewBag();
            return View(model);
        }

        _logger.LogInformation("Updating event {EventId}: {EventName}", id, model.EventName);
        try
        {
            var eventModel = ConvertFormViewModelToEvent(model);
            if (!string.IsNullOrWhiteSpace(model.EventTime))
            {
                if (TimeSpan.TryParse(model.EventTime, out var timeSpan))
                    eventModel.EventTime = timeSpan.ToString(@"hh\:mm");
                else
                    eventModel.EventTime = model.EventTime;
            }
            eventModel.EventId = id;

            var success = await _eventsApiService.UpdateEventAsync(id, eventModel);
            if (success)
            {
                _logger.LogInformation("Successfully updated event {EventId}", id);
                TempData["Success"] = "Event updated successfully!";
                return RedirectToAction(nameof(Details), new { id });
            }
            else
            {
                _logger.LogWarning("Failed to update event {EventId}", id);
                ModelState.AddModelError(string.Empty, "Unable to update event. Please try again.");
                await PopulateCategoriesViewBag();
                return View(model);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating event {EventId}", id);
            throw;
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            return NotFound();

        _logger.LogInformation("Deleting event {EventId}", id);
        try
        {
            var success = await _eventsApiService.DeleteEventAsync(id);
            if (success)
            {
                _logger.LogInformation("Successfully deleted event {EventId}", id);
                TempData["Success"] = "Event deleted successfully!";
            }
            else
            {
                _logger.LogWarning("Failed to delete event {EventId}", id);
                TempData["Error"] = "Unable to delete event. Please try again.";
            }
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting event {EventId}", id);
            throw;
        }
    }

    private async Task PopulateCategoriesViewBag()
    {
        try
        {
            var categories = await _eventsApiService.GetAllCategoriesAsync();
            ViewBag.Categories = new SelectList(categories, "CategoryId", "Name");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to load categories for dropdown");
            ViewBag.Categories = new SelectList(Enumerable.Empty<Category>(), "CategoryId", "Name");
        }
    }

    private static Event ConvertFormViewModelToEvent(EventFormViewModel model)
    {
        return new Event
        {
            EventId = model.EventId ?? string.Empty,
            EventName = model.EventName,
            CategoryId = model.EventCategory,
            Description = model.Description,
            EventDate = model.EventDate,
            EventTime = model.EventTime,
            Location = model.Location,
            VenueName = !string.IsNullOrWhiteSpace(model.VenueName) ? model.VenueName : "Venue TBD",
            Capacity = model.Capacity,
            OwnerId = "system",
            OwnerName = "System User",
            OwnerEmail = "system@eventapp.com",
            Status = "Draft",
            TicketsSold = 0
        };
    }

    private List<TicketCategory> GenerateMockTickets(string categoryId)
    {
        var tickets = new List<TicketCategory>();

        if (categoryId?.Contains("music", StringComparison.OrdinalIgnoreCase) == true)
        {
            tickets.Add(new TicketCategory 
            { 
                Category = "General Admission", 
                Price = 49.99m, 
                AvailableQuantity = 100,
                Description = "Standard entry to the concert"
            });
            tickets.Add(new TicketCategory 
            { 
                Category = "VIP Experience", 
                Price = 149.99m, 
                AvailableQuantity = 25,
                Description = "Premium viewing area with complimentary drinks"
            });
        }
        else if (categoryId?.Contains("conference", StringComparison.OrdinalIgnoreCase) == true)
        {
            tickets.Add(new TicketCategory 
            { 
                Category = "Standard Pass", 
                Price = 199.99m, 
                AvailableQuantity = 250,
                Description = "Full conference access"
            });
            tickets.Add(new TicketCategory 
            { 
                Category = "Workshop Pass", 
                Price = 299.99m, 
                AvailableQuantity = 50,
                Description = "Conference access plus hands-on workshops"
            });
        }
        else
        {
            tickets.Add(new TicketCategory 
            { 
                Category = "Standard Entry", 
                Price = 29.99m, 
                AvailableQuantity = 100,
                Description = "General admission"
            });
            tickets.Add(new TicketCategory 
            { 
                Category = "Premium Experience", 
                Price = 79.99m, 
                AvailableQuantity = 30,
                Description = "Enhanced experience with premium benefits"
            });
        }

        tickets.Add(new TicketCategory 
        { 
            Category = "Early Bird Special", 
            Price = 19.99m, 
            AvailableQuantity = Math.Max(10, new Random().Next(5, 30)),
            Description = "Limited availability - book early and save!"
        });

        return tickets;
    }
}