using EventsWebApp.Models.Domain;
using EventsWebApp.Models.DTOs;
using EventsWebApp.Services.Interfaces;
using System.Text.Json;

namespace EventsWebApp.Services.Implementation;
public class EventsApiService(
    IHttpClientFactory httpClientFactory,
    IModelMappingService mappingService,
    IConfigurationService configurationService,
    IErrorHandlingService errorHandlingService,
    ILogger<EventsApiService> logger) : IEventsApiService
{
    private readonly HttpClient _httpClient = httpClientFactory?.CreateClient("EventsApi") ?? throw new ArgumentNullException(nameof(httpClientFactory));
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
            var pagedResponse = JsonSerializer.Deserialize<PagedEventsResponse>(jsonContent, GetJsonOptions());

            if (pagedResponse == null)
            {
                _logger.LogWarning("API returned null response after deserialization");
                return Enumerable.Empty<Event>();
            }

            var eventDtos = pagedResponse.Items ?? new List<EventDto>();
            _logger.LogInformation("Successfully retrieved {Count} events", eventDtos.Count);
        
            var events = eventDtos.Select(dto => _mappingService.MapToEvent(dto));
            return events;
        }
        catch (JsonException jsonEx)
        {
            _logger.LogError(jsonEx, "JSON deserialization failed");
            await _errorHandlingService.LogErrorAsync("Failed to parse events JSON response", jsonEx);
            return Enumerable.Empty<Event>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while fetching events");
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

            if (!int.TryParse(eventModel.CategoryId, out var categoryId))
            {
                _logger.LogError("Invalid CategoryId '{CategoryId}' - must be a valid integer", eventModel.CategoryId);
                return false;
            }

            var createDto = new CreateEventDto
            {
                EventName = eventModel.EventName,
                CategoryId = categoryId,
                Description = eventModel.Description,
                OwnerId = eventModel.OwnerId,
                OwnerName = eventModel.OwnerName,
                OwnerEmail = eventModel.OwnerEmail,
                Location = eventModel.Location,
                VenueName = !string.IsNullOrWhiteSpace(eventModel.VenueName) 
                    ? eventModel.VenueName 
                    : "Venue TBD",
                EventDate = eventModel.EventDate,
                EventTime = eventModel.EventTime,
                Capacity = eventModel.Capacity,
                Status = eventModel.Status
            };

            if (!IsValidEventDto(createDto))
            {
                _logger.LogWarning("Event validation failed for {EventName}", eventModel.EventName);
                return false;
            }

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
                _logger.LogWarning("Failed to create event {EventName}: {ErrorMessage}", eventModel.EventName, errorMessage);
                return false;
            }
        }
        catch (HttpRequestException httpEx)
        {
            _logger.LogError(httpEx, "Network error creating event {EventName}", eventModel.EventName);
            await _errorHandlingService.LogErrorAsync($"Network error creating event {eventModel.EventName}", httpEx);
            return false;
        }
        catch (JsonException jsonEx)
        {
            _logger.LogError(jsonEx, "JSON serialization error creating event {EventName}", eventModel.EventName);
            await _errorHandlingService.LogErrorAsync($"JSON error creating event {eventModel.EventName}", jsonEx);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error creating event {EventName}", eventModel.EventName);
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
                EventName = eventModel.EventName,
                EventCategory = eventModel.CategoryId,
                Description = eventModel.Description,
                Location = eventModel.Location,
                VenueName = eventModel.VenueName,
                EventDate = eventModel.EventDate,
                EventTime = eventModel.EventTime,
                Capacity = eventModel.Capacity,
                Status = eventModel.Status
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
                var errorMessage = _errorHandlingService.HandleApiError(response);
                _logger.LogWarning("Failed to retrieve categories: {ErrorMessage}", errorMessage);
                return Enumerable.Empty<Category>();
            }

            var jsonContent = await response.Content.ReadAsStringAsync();
            var categoryDtos = JsonSerializer.Deserialize<List<CategoryDto>>(jsonContent, GetJsonOptions())
                              ?? [];

            _logger.LogInformation("Successfully retrieved {Count} categories", categoryDtos.Count);

            return categoryDtos.Select(dto => _mappingService.MapToCategory(dto));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred while fetching categories");
            await _errorHandlingService.LogErrorAsync("Error retrieving categories", ex);
            return [];
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

    private static bool IsValidEventDto(CreateEventDto dto)
    {
        return !string.IsNullOrWhiteSpace(dto.EventName) &&
               dto.CategoryId > 0 &&
               !string.IsNullOrWhiteSpace(dto.OwnerId) &&
               !string.IsNullOrWhiteSpace(dto.OwnerName) &&
               !string.IsNullOrWhiteSpace(dto.OwnerEmail) &&
               !string.IsNullOrWhiteSpace(dto.Location) &&
               !string.IsNullOrWhiteSpace(dto.VenueName) &&
               !string.IsNullOrWhiteSpace(dto.EventDate) &&
               !string.IsNullOrWhiteSpace(dto.EventTime) &&
               dto.Capacity >= 0 &&
               IsValidEmail(dto.OwnerEmail);
    }

    private static bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
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