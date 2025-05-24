using EventsWebApp.Models.Domain;
using EventsWebApp.Models.ViewModels;
using EventsWebApp.Services.Interfaces;
using EventsWebApp.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace EventsWebApp.Controllers;

public class HomeController(
    IEventsApiService eventsApiService,
    IModelMappingService mappingService,
    ILogger<HomeController> logger) : Controller
{
    private readonly IEventsApiService _eventsApiService = eventsApiService;
    private readonly IModelMappingService _mappingService = mappingService;
    private readonly ILogger<HomeController> _logger = logger;

    public async Task<IActionResult> Index()
    {
        _logger.LogInformation("Loading dashboard page");

        try
        {
            var events = await _eventsApiService.GetAllEventsAsync();

            var cardViewModels = events
                .Select(e => _mappingService.MapToEventCardViewModel(e))
                .Where(vm => vm.IsUpcoming)
                .OrderBy(vm => vm.FullEventDateTime)
                .Take(10)
                .ToList();

            _logger.LogInformation("Dashboard loaded with {EventCount} upcoming events", cardViewModels.Count);

            return View(cardViewModels);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading dashboard");

            TempData["Warning"] = "Unable to load upcoming events. Please try again later.";
            return View(new List<EventCardViewModel>());
        }
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        var errorMessage = TempData["ErrorMessage"]?.ToString();
        var statusCode = TempData["StatusCode"]?.ToString();

        var errorViewModel = new ErrorViewModel
        {
            RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier,
            ErrorMessage = errorMessage,
            UserFriendlyMessage = GetUserFriendlyErrorMessage(statusCode)
        };

        return View(errorViewModel);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult NotFoundError()
    {
        var errorViewModel = new ErrorViewModel
        {
            RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier,
            UserFriendlyMessage = "The page you requested could not be found."
        };

        Response.StatusCode = 404;
        return View("Error", errorViewModel);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public new IActionResult Unauthorized()
    {
        var errorViewModel = new ErrorViewModel
        {
            RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier,
            UserFriendlyMessage = "You are not authorized to access this resource."
        };

        Response.StatusCode = 401;
        return View("Error", errorViewModel);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Forbidden()
    {
        var errorViewModel = new ErrorViewModel
        {
            RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier,
            UserFriendlyMessage = "Access to this resource is forbidden."
        };

        Response.StatusCode = 403;
        return View("Error", errorViewModel);
    }

    [HttpGet]
    public async Task<IActionResult> Health()
    {
        try
        {
            var isApiHealthy = await _eventsApiService.IsApiHealthyAsync();

            var healthStatus = new
            {
                Status = isApiHealthy ? "Healthy" : "Degraded",
                Timestamp = DateTime.UtcNow,
                Dependencies = new
                {
                    EventsApi = isApiHealthy ? "Healthy" : "Unhealthy"
                }
            };

            if (isApiHealthy)
            {
                return Ok(healthStatus);
            }
            else
            {
                Response.StatusCode = 503; // Service Unavailable
                return Json(healthStatus);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Health check failed");

            var healthStatus = new
            {
                Status = "Unhealthy",
                Timestamp = DateTime.UtcNow,
                Error = "Health check failed"
            };

            Response.StatusCode = 503;
            return Json(healthStatus);
        }
    }

    private static string GetUserFriendlyErrorMessage(string? statusCode)
    {
        return statusCode switch
        {
            "400" => "There was a problem with your request. Please check your input and try again.",
            "401" => "You need to log in to access this resource.",
            "403" => "You don't have permission to access this resource.",
            "404" => "The page you're looking for could not be found.",
            "500" => "We're experiencing technical difficulties. Please try again later.",
            "502" => "We're having trouble connecting to our services. Please try again later.",
            "503" => "Our services are temporarily unavailable. Please try again later.",
            _ => "An unexpected error occurred. Please try again later."
        };
    }
}