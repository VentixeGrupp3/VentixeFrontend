using EventsWebApp.Models.Domain;
using EventsWebApp.Models.DTOs;
using EventsWebApp.Services.Interfaces;
using System.Text.Json;

namespace EventsWebApp.Services.Implementation;
public class EventsApiService(
    HttpClient httpClient,
    IModelMappingService mappingService,
    IConfigurationService configurationService,
    IErrorHandlingService errorHandlingService,
    ILogger<EventsApiService> logger) : IEventsApiService
{
    private readonly HttpClient _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    private readonly IModelMappingService _mappingService = mappingService ?? throw new ArgumentNullException(nameof(mappingService));
    private readonly IConfigurationService _configurationService = configurationService ?? throw new ArgumentNullException(nameof(configurationService));
    private readonly IErrorHandlingService _errorHandlingService = errorHandlingService ?? throw new ArgumentNullException(nameof(errorHandlingService));
    private readonly ILogger<EventsApiService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task<IEnumerable<Event>> GetAllEventsAsync()
    {
        try
        {
            _logger.LogInformation("Fetching all events from API");

            var response = await _httpClient.GetAsync("api/events");

            if (!response.IsSuccessStatusCode)
            {

                var errorMessage = _errorHandlingService.HandleApiError(response);
                _logger.LogWarning("API request failed: {ErrorMessage}", errorMessage);
                return Enumerable.Empty<Event>();
            }

            var jsonContent = await response.Content.ReadAsStringAsync();
            var eventDtos = JsonSerializer.Deserialize<List<EventDto>>(jsonContent, GetJsonOptions())
                           ?? new List<EventDto>();

            _logger.LogInformation("Successfully retrieved {Count} events from API", eventDtos.Count);

            var events = eventDtos.Select(dto => _mappingService.MapToEvent(dto));
            return events;
        }
        catch (HttpRequestException ex)
        {
            await _errorHandlingService.LogErrorAsync("Network error while fetching events", ex);
            return Enumerable.Empty<Event>();
        }
        catch (JsonException ex)
        {
            await _errorHandlingService.LogErrorAsync("Failed to parse events JSON response", ex);
            return Enumerable.Empty<Event>();
        }
        catch (Exception ex)
        {
            await _errorHandlingService.LogErrorAsync("Unexpected error while fetching events", ex);
            return Enumerable.Empty<Event>();
        }
    }
    public async Task<Event?> GetEventByIdAsync(string eventId)
    {
        if (string.IsNullOrWhiteSpace(eventId))
        {
            _logger.LogWarning("GetEventByIdAsync called with null or empty eventId");
            return null;
        }

        try
        {
            _logger.LogInformation("Fetching event {EventId} from API", eventId);

            var response = await _httpClient.GetAsync($"api/events/{eventId}");

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _logger.LogInformation("Event {EventId} not found", eventId);
                return null;
            }

            if (!response.IsSuccessStatusCode)
            {
                var errorMessage = _errorHandlingService.HandleApiError(response);
                _logger.LogWarning("Failed to retrieve event {EventId}: {ErrorMessage}", eventId, errorMessage);
                return null;
            }

            var jsonContent = await response.Content.ReadAsStringAsync();
            var eventDto = JsonSerializer.Deserialize<EventDto>(jsonContent, GetJsonOptions());

            if (eventDto == null)
            {
                _logger.LogWarning("API returned null event data for {EventId}", eventId);
                return null;
            }

            _logger.LogInformation("Successfully retrieved event {EventId}", eventId);
            return _mappingService.MapToEvent(eventDto);
        }
        catch (Exception ex)
        {
            await _errorHandlingService.LogErrorAsync($"Error retrieving event {eventId}", ex);
            return null;
        }
    }

    public async Task<bool> CreateEventAsync(Event eventModel)
    {
        if (eventModel == null)
        {
            _logger.LogWarning("CreateEventAsync called with null event model");
            return false;
        }

        try
        {
            _logger.LogInformation("Creating new event: {EventName}", eventModel.EventName);

            var createDto = new CreateEventDto
            {
                EventName = eventModel.EventName,
                CategoryId = eventModel.CategoryId,
                Description = eventModel.Description,
                OwnerId = eventModel.OwnerId,
                OwnerName = eventModel.OwnerName,
                OwnerEmail = eventModel.OwnerEmail,
                Location = eventModel.Location,
                VenueName = eventModel.VenueName ?? string.Empty,
                EventDate = eventModel.EventDate,
                EventTime = eventModel.EventTime,
                Capacity = eventModel.Capacity,
                Status = eventModel.Status
            };

            var jsonContent = JsonSerializer.Serialize(createDto, GetJsonOptions());
            var content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("api/events", content);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Successfully created event: {EventName}", eventModel.EventName);
                return true;
            }
            else
            {
                var errorMessage = _errorHandlingService.HandleApiError(response);
                _logger.LogWarning("Failed to create event {EventName}: {ErrorMessage}", 
                                  eventModel.EventName, errorMessage);
                return false;
            }
        }
        catch (Exception ex)
        {
            await _errorHandlingService.LogErrorAsync($"Error creating event {eventModel.EventName}", ex);
            return false;
        }
    }

    public async Task<bool> UpdateEventAsync(string eventId, Event eventModel)
    {
        if (string.IsNullOrWhiteSpace(eventId) || eventModel == null)
        {
            _logger.LogWarning("UpdateEventAsync called with invalid parameters");
            return false;
        }

        try
        {
            _logger.LogInformation("Updating event {EventId}: {EventName}", eventId, eventModel.EventName);

            var updateDto = new UpdateEventDto
            {
                Title = eventModel.EventName,
                Date = eventModel.GetEventDateTime(),
                Location = eventModel.Location,
                Description = eventModel.Description,
                CategoryId = eventModel.CategoryId
            };

            var jsonContent = JsonSerializer.Serialize(updateDto, GetJsonOptions());
            var content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");

            var response = await _httpClient.PutAsync($"api/events/{eventId}", content);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Successfully updated event {EventId}", eventId);
                return true;
            }
            else
            {
                var errorMessage = _errorHandlingService.HandleApiError(response);
                _logger.LogWarning("Failed to update event {EventId}: {ErrorMessage}", eventId, errorMessage);
                return false;
            }
        }
        catch (Exception ex)
        {
            await _errorHandlingService.LogErrorAsync($"Error updating event {eventId}", ex);
            return false;
        }
    }

    public async Task<bool> DeleteEventAsync(string eventId)
    {
        if (string.IsNullOrWhiteSpace(eventId))
        {
            _logger.LogWarning("DeleteEventAsync called with null or empty eventId");
            return false;
        }

        try
        {
            _logger.LogInformation("Deleting event {EventId}", eventId);

            var response = await _httpClient.DeleteAsync($"api/events/{eventId}");

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Successfully deleted event {EventId}", eventId);
                return true;
            }
            else
            {
                var errorMessage = _errorHandlingService.HandleApiError(response);
                _logger.LogWarning("Failed to delete event {EventId}: {ErrorMessage}", eventId, errorMessage);
                return false;
            }
        }
        catch (Exception ex)
        {
            await _errorHandlingService.LogErrorAsync($"Error deleting event {eventId}", ex);
            return false;
        }
    }

    public async Task<IEnumerable<Category>> GetAllCategoriesAsync()
    {
        try
        {
            _logger.LogInformation("Fetching categories from API");

            var response = await _httpClient.GetAsync("api/categories");

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Primary categories endpoint failed, trying alternative");
                response = await _httpClient.GetAsync("api/categoryentity");
            }

            if (!response.IsSuccessStatusCode)
            {
                var errorMessage = _errorHandlingService.HandleApiError(response);
                _logger.LogWarning("Failed to retrieve categories: {ErrorMessage}", errorMessage);
                return Enumerable.Empty<Category>();
            }

            var jsonContent = await response.Content.ReadAsStringAsync();
            var categoryDtos = JsonSerializer.Deserialize<List<CategoryDto>>(jsonContent, GetJsonOptions())
                              ?? new List<CategoryDto>();

            _logger.LogInformation("Successfully retrieved {Count} categories", categoryDtos.Count);

            return categoryDtos.Select(dto => _mappingService.MapToCategory(dto));
        }
        catch (Exception ex)
        {
            await _errorHandlingService.LogErrorAsync("Error retrieving categories", ex);
            return Enumerable.Empty<Category>();
        }
    }
    public async Task<bool> IsApiHealthyAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("api/health");
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    private static JsonSerializerOptions GetJsonOptions()
    {
        return new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };
    }
}