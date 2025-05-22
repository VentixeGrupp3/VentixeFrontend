using EventsWebApp.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using EventsWebApp.ViewModels;

namespace EventsWebApp.Controllers;

public class HomeController(
    IHttpClientFactory httpFactory,
    ILogger<HomeController> logger) : Controller
{
    private readonly HttpClient _api = httpFactory.CreateClient("EventsApi");
    private readonly ILogger<HomeController> _logger = logger;

    public async Task<IActionResult> Index()
    {
        List<EventDto> events;

        try
        {
            events = await _api.GetFromJsonAsync<List<EventDto>>("api/events")
                         ?? new List<EventDto>();
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Error fetching events from backend");
            TempData["Error"] = "Unable to load events. Please try again.";
            events = new List<EventDto>();
        }

        var viewModel = events.Select(e => new EventCardViewModel
        {
            EventId = e.EventId,
            EventName = e.Title,
            EventDate = e.Date.Date,
            EventTime = e.Date.TimeOfDay,
            Location = e.Location,
        }).ToList();

        return View(viewModel);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel
        {
            RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
        });
    }
}
