using EventsWebApp.Models;
using EventsWebApp.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace EventsWebApp.Controllers;

public record EventDto(string EventId, string Title, DateTime Date, string Location, int SeatsAvailable);
public record CategoryDto(string CategoryId, string Name);
public record UpdateEventDto(string Title, DateTime Date, string Location, string Description, string CategoryId);

public class EventsController(
    IHttpClientFactory httpFactory,
    ILogger<EventsController> logger) : Controller
{
    private readonly HttpClient _api = httpFactory.CreateClient("EventsApi");
    private readonly ILogger<EventsController> _logger = logger;


    public async Task<IActionResult> Index()
    {
        List<EventListViewModel> eventViewModels = new();
        List<CategoryDto> categories = new();
    
        try
        {
            try
            {
                string[] categoryEndpoints = { "api/categoryentity" };
            
                foreach (var endpoint in categoryEndpoints)
                {
                    try
                    {
                        _logger.LogInformation("Attempting to fetch categories from {Endpoint}", endpoint);
                        var categoriesResponse = await _api.GetAsync(endpoint);
                    
                        if (categoriesResponse.IsSuccessStatusCode)
                        {
                            categories = await categoriesResponse.Content.ReadFromJsonAsync<List<CategoryDto>>() 
                                      ?? new List<CategoryDto>();
                            _logger.LogInformation("Successfully loaded {Count} categories from {Endpoint}", 
                                                categories.Count, endpoint);
                            break;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Error trying endpoint {Endpoint}", endpoint);
                    }
                }
            
                if (categories.Count == 0)
                {
                    _logger.LogWarning("Failed to load categories from any endpoint");
                    TempData["Warning"] = "Categories could not be loaded. Events will be shown without category information.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error fetching categories. Using empty categories list.");
            }
        
            ViewBag.Categories = categories;
        
            // Get events - continue even if categories failed
            try
            {
                var eventsResponse = await _api.GetAsync("api/events");
                if (!eventsResponse.IsSuccessStatusCode)
                {
                    string errorContent = await eventsResponse.Content.ReadAsStringAsync();
                    _logger.LogError("Events API returned {StatusCode}: {ErrorContent}", 
                                   eventsResponse.StatusCode, errorContent);
                
                    TempData["Error"] = "Unable to load events from the server.";
                    return View(eventViewModels);
                }
            
                // Deserialize events and map to view models
                var events = await eventsResponse.Content.ReadFromJsonAsync<List<EventViewModel>>() 
                          ?? new List<EventViewModel>();
            
                eventViewModels = events.Select(e => new EventListViewModel
                {
                    EventId = e.EventId,
                    EventName = e.EventName,
                    EventCategory = categories.FirstOrDefault(c => c.CategoryId == e.CategoryId)?.Name ?? "Uncategorized",
                    Description = e.Description ?? string.Empty,
                    EventDate = e.EventDate,
                    EventTime = e.EventTime,
                    Location = e.Location,
                    VenueName = e.VenueName ?? string.Empty,
                    Capacity = e.Capacity
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching events");
                TempData["Error"] = "Unable to load events from the server.";
            }
        
            return View(eventViewModels);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in Index action");
            TempData["Error"] = "An error occurred while loading events. Please try again later.";
            return View(new List<EventFormViewModel>());
        }
    }

    public async Task<IActionResult> Details(string id)
    {
        try
        {
            var evt = await _api.GetFromJsonAsync<EventFormViewModel>($"api/events/{id}");
                
            if (evt == null) return NotFound();
            
            // Add mock ticket categories if none exist
            evt.TicketCategories = GenerateMockTickets(evt.EventCategory);
            
            return View(evt);
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return NotFound();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching event {EventId}", id);
            TempData["Error"] = "Unable to load event details.";
            return RedirectToAction(nameof(Index));
        }
    }
    

    public async Task<IActionResult> Create()
    {
        var categories = await _api.GetFromJsonAsync<List<CategoryDto>>("api/categories");
        if (categories == null)
        {
            categories = new List<CategoryDto>();
            _logger.LogWarning("Categories list from API was null, replaced with empty list.");
        }

        ViewBag.Categories = new SelectList(categories, "CategoryId", "Name");

        return View();
    }


    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateEventDto dto)
    {
        async Task RepopulateCategories()
        {
            var cats = await _api.GetFromJsonAsync<List<CategoryDto>>("api/categories")
                       ?? new List<CategoryDto>();
            ViewBag.Categories = new SelectList(cats,"CategoryId", "Name");
        }
        if (!ModelState.IsValid)
        {
            await RepopulateCategories();
            return View(dto);
        }

        try
        {
            var resp = await _api.PostAsJsonAsync("api/events", dto);
            resp.EnsureSuccessStatusCode();

            TempData["Success"] = "Event created successfully!";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating event");
            ModelState.AddModelError(string.Empty, "Unable to create event. Please try again.");
            await RepopulateCategories();
            return View(dto);
        }
    }


    public async Task<IActionResult> Edit(string id)
    {
        try
        {
            var evt = await _api.GetFromJsonAsync<EventDto>($"api/events/{id}");
            if (evt == null) return NotFound();

            ViewBag.Categories = await _api.GetFromJsonAsync<List<CategoryDto>>("api/categories")
                                ?? new List<CategoryDto>();

            var editModel = new UpdateEventDto(
                Title: evt.Title,
                Date: evt.Date,
                Location: evt.Location,
                Description: "",
                CategoryId: ""
            );

            return View(editModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading event for edit");
            TempData["Error"] = "Unable to load event for editing.";
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(string id, UpdateEventDto dto)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Categories = await _api.GetFromJsonAsync<List<CategoryDto>>("api/categories")
                                ?? new List<CategoryDto>();
            return View(dto);
        }

        try
        {
            var resp = await _api.PutAsJsonAsync($"api/events/{id}", dto);
            resp.EnsureSuccessStatusCode();

            TempData["Success"] = "Event updated successfully!";
            return RedirectToAction(nameof(Details), new { id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating event");
            ModelState.AddModelError("", "Unable to update event. Please try again.");
            ViewBag.Categories = await _api.GetFromJsonAsync<List<CategoryDto>>("api/categories")
                                ?? new List<CategoryDto>();
            return View(dto);
        }
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(string id)
    {
        try
        {
            var resp = await _api.DeleteAsync($"api/events/{id}");
            resp.EnsureSuccessStatusCode();
            TempData["Success"] = "Event deleted successfully!";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting event");
            TempData["Error"] = "Unable to delete event. Please try again.";
        }

        return RedirectToAction(nameof(Index));
    }

    private List<TicketCategoryDto> GenerateMockTickets(string category)
    {
        // Create different ticket types based on event category
        var tickets = new List<TicketCategoryDto>();
            
        if (category?.Contains("Music") == true)
        {
            tickets.Add(new TicketCategoryDto 
            { 
                TicketCategory = "General Admission", 
                Price = 49.99m, 
                AvailableQuantity = 100,
                Description = "Standard entry to the concert"
            });
                
            tickets.Add(new TicketCategoryDto 
            { 
                TicketCategory = "VIP Experience", 
                Price = 149.99m, 
                AvailableQuantity = 25,
                Description = "Premium viewing area with complimentary drinks"
            });
        }
        else if (category?.Contains("Conference") == true)
        {
            tickets.Add(new TicketCategoryDto 
            { 
                TicketCategory = "Standard Pass", 
                Price = 199.99m, 
                AvailableQuantity = 250,
                Description = "Full conference access"
            });
                
            tickets.Add(new TicketCategoryDto 
            { 
                TicketCategory = "Workshop Pass", 
                Price = 299.99m, 
                AvailableQuantity = 50,
                Description = "Conference access plus hands-on workshops"
            });
        }
        else
        {
            // Default tickets for any other event type
            tickets.Add(new TicketCategoryDto 
            { 
                TicketCategory = "Standard Entry", 
                Price = 29.99m, 
                AvailableQuantity = 100,
                Description = "General admission"
            });
                
            tickets.Add(new TicketCategoryDto 
            { 
                TicketCategory = "Premium Experience", 
                Price = 79.99m, 
                AvailableQuantity = 30,
                Description = "Enhanced experience with premium benefits"
            });
        }
            
        // Always add early bird discount option
        tickets.Add(new TicketCategoryDto 
        { 
            TicketCategory = "Early Bird Special", 
            Price = 19.99m, 
            AvailableQuantity = Math.Max(10, new Random().Next(5, 30)),
            Description = "Limited availability - book early and save!"
        });
            
        return tickets;
    }
}
