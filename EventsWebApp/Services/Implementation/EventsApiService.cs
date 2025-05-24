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
            _logger.LogInformation("Starting to fetch all events from API");
            var response = await _httpClient.GetAsync("api/events");
        
            if (!response.IsSuccessStatusCode)
            {
                var errorMessage = _errorHandlingService.HandleApiError(response);
                _logger.LogWarning("API request failed: {ErrorMessage}", errorMessage);
                return Enumerable.Empty<Event>();
            }
        
            var jsonContent = await response.Content.ReadAsStringAsync();
            _logger.LogInformation("Raw JSON Response: {JsonContent}", jsonContent);
        
            _logger.LogInformation("JSON Response Length: {Length} characters", jsonContent.Length);
        
            var preview = jsonContent.Length > 150 ? jsonContent.Substring(0, 150) + "..." : jsonContent;
            _logger.LogInformation("JSON Preview (first 150 chars): {Preview}", preview);
        
            try
            {
                var genericObject = JsonSerializer.Deserialize<JsonElement>(jsonContent);
                _logger.LogInformation("JSON root element type: {Kind}", genericObject.ValueKind);
            
                if (genericObject.ValueKind == JsonValueKind.Object)
                {
                    var properties = genericObject.EnumerateObject().Select(p => p.Name).ToList();
                    _logger.LogInformation("Available JSON properties: [{Properties}]", string.Join(", ", properties));
                
                    var hasItems = properties.Any(p => p.Equals("items", StringComparison.OrdinalIgnoreCase));
                    var hasTotalCount = properties.Any(p => p.Equals("totalCount", StringComparison.OrdinalIgnoreCase) || 
                                                           p.Equals("total", StringComparison.OrdinalIgnoreCase));
                    _logger.LogInformation("Pagination indicators - Has items property: {HasItems}, Has count property: {HasCount}", 
                                          hasItems, hasTotalCount);
                }
                else if (genericObject.ValueKind == JsonValueKind.Array)
                {
                    _logger.LogInformation("Response is a direct array with {Count} elements", genericObject.GetArrayLength());
                }
            }
            catch (Exception diagnosticEx)
            {
                _logger.LogError(diagnosticEx, "Failed to analyze JSON structure - this might indicate malformed JSON");
            }
        
            var pagedResponse = JsonSerializer.Deserialize<PagedEventsResponse>(jsonContent, GetJsonOptions());
    
            if (pagedResponse == null)
            {
                _logger.LogWarning("API returned null response after deserialization attempt");
                return Enumerable.Empty<Event>();
            }
        
            _logger.LogInformation("Retrieved page {CurrentPage} of {TotalPages}, showing {ItemCount} of {TotalCount} total events", 
                                  pagedResponse.CurrentPage, pagedResponse.TotalPages, 
                                  pagedResponse.ItemCount, pagedResponse.TotalCount);
        
            var eventDtos = pagedResponse.Items ?? new List<EventDto>();
    
            _logger.LogInformation("Successfully extracted {Count} events from paginated response", eventDtos.Count);
        
            var events = eventDtos.Select(dto => _mappingService.MapToEvent(dto));
            return events;
        }
        catch (JsonException jsonEx)
        {
            _logger.LogError(jsonEx, "JSON deserialization failed - this tells us there's a mismatch between expected and actual structure");
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
            _logger.LogInformation("=== EVENT CREATION DIAGNOSTIC START ===");
            _logger.LogInformation("Creating new event: {EventName}", eventModel.EventName);

            _logger.LogInformation("Input Event Model Analysis:");
            _logger.LogInformation("  - EventName: '{EventName}'", eventModel.EventName);
            _logger.LogInformation("  - CategoryId (CRITICAL): '{CategoryId}' (Type: {CategoryIdType})", 
                eventModel.CategoryId, eventModel.CategoryId?.GetType().Name ?? "null");
            _logger.LogInformation("  - Description: '{Description}'", eventModel.Description ?? "null");
            _logger.LogInformation("  - OwnerId: '{OwnerId}'", eventModel.OwnerId);
            _logger.LogInformation("  - OwnerName: '{OwnerName}'", eventModel.OwnerName);
            _logger.LogInformation("  - OwnerEmail: '{OwnerEmail}'", eventModel.OwnerEmail);
            _logger.LogInformation("  - Location: '{Location}'", eventModel.Location);
            _logger.LogInformation("  - VenueName: '{VenueName}'", eventModel.VenueName ?? "null");
            _logger.LogInformation("  - EventDate: '{EventDate}'", eventModel.EventDate);
            _logger.LogInformation("  - EventTime: '{EventTime}'", eventModel.EventTime);
            _logger.LogInformation("  - Capacity: {Capacity}", eventModel.Capacity);
            _logger.LogInformation("  - Status: '{Status}'", eventModel.Status);

            _logger.LogInformation("=== CATEGORY ID CONVERSION ANALYSIS ===");
            
            var categoryConversionSuccess = int.TryParse(eventModel.CategoryId, out var categoryId);
            _logger.LogInformation("CategoryId conversion attempt:");
            _logger.LogInformation("  - Original value: '{OriginalValue}'", eventModel.CategoryId);
            _logger.LogInformation("  - Conversion successful: {ConversionSuccess}", categoryConversionSuccess);
            _logger.LogInformation("  - Converted value: {ConvertedValue}", categoryId);
            
            if (!categoryConversionSuccess)
            {
                _logger.LogError("CRITICAL ISSUE: CategoryId '{CategoryId}' cannot be converted to integer. This will cause API validation to fail.", eventModel.CategoryId);
                _logger.LogError("DIAGNOSIS: Your form is sending category names (like 'music') but the API expects category IDs (like 1, 2, 3).");
                _logger.LogError("SOLUTION NEEDED: We need to map category names to their corresponding integer IDs.");
                
                await AnalyzeCategoryMappingAsync();
                
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

            _logger.LogInformation("=== DTO VALIDATION ANALYSIS ===");
            var validationErrors = new List<string>();
            
            if (string.IsNullOrWhiteSpace(createDto.EventName))
                validationErrors.Add("EventName is required");
            else if (createDto.EventName.Length > 200)
                validationErrors.Add($"EventName exceeds 200 characters (current: {createDto.EventName.Length})");
                
            if (createDto.CategoryId <= 0)
                validationErrors.Add($"CategoryId must be greater than 0 (current: {createDto.CategoryId})");
                
            if (!string.IsNullOrEmpty(createDto.Description) && createDto.Description.Length > 2000)
                validationErrors.Add($"Description exceeds 2000 characters (current: {createDto.Description.Length})");
                
            if (string.IsNullOrWhiteSpace(createDto.OwnerId))
                validationErrors.Add("OwnerId is required");
            else if (createDto.OwnerId.Length > 100)
                validationErrors.Add($"OwnerId exceeds 100 characters (current: {createDto.OwnerId.Length})");
                
            if (string.IsNullOrWhiteSpace(createDto.OwnerName))
                validationErrors.Add("OwnerName is required");
            else if (createDto.OwnerName.Length > 200)
                validationErrors.Add($"OwnerName exceeds 200 characters (current: {createDto.OwnerName.Length})");
                
            if (string.IsNullOrWhiteSpace(createDto.OwnerEmail))
                validationErrors.Add("OwnerEmail is required");
            else if (createDto.OwnerEmail.Length > 256)
                validationErrors.Add($"OwnerEmail exceeds 256 characters (current: {createDto.OwnerEmail.Length})");
            else if (!IsValidEmail(createDto.OwnerEmail))
                validationErrors.Add($"OwnerEmail format is invalid: {createDto.OwnerEmail}");
                
            if (string.IsNullOrWhiteSpace(createDto.Location))
                validationErrors.Add("Location is required");
            else if (createDto.Location.Length > 200)
                validationErrors.Add($"Location exceeds 200 characters (current: {createDto.Location.Length})");
                
            if (string.IsNullOrWhiteSpace(createDto.VenueName))
                validationErrors.Add("VenueName is required");
            else if (createDto.VenueName.Length > 200)
                validationErrors.Add($"VenueName exceeds 200 characters (current: {createDto.VenueName.Length})");
                
            if (string.IsNullOrWhiteSpace(createDto.EventDate))
                validationErrors.Add("EventDate is required");
            else if (!System.Text.RegularExpressions.Regex.IsMatch(createDto.EventDate, @"^\d{4}-\d{2}-\d{2}$"))
                validationErrors.Add($"EventDate format is invalid. Expected YYYY-MM-DD, got: {createDto.EventDate}");
                
            if (string.IsNullOrWhiteSpace(createDto.EventTime))
                validationErrors.Add("EventTime is required");
            else if (!System.Text.RegularExpressions.Regex.IsMatch(createDto.EventTime, @"^([01]\d|2[0-3]):([0-5]\d)$"))
                validationErrors.Add($"EventTime format is invalid. Expected HH:MM in 24-hour format, got: {createDto.EventTime}");
                
            if (createDto.Capacity < 0)
                validationErrors.Add($"Capacity cannot be negative (current: {createDto.Capacity})");

            if (validationErrors.Any())
            {
                _logger.LogError("DTO VALIDATION FAILED. Errors found:");
                foreach (var error in validationErrors)
                {
                    _logger.LogError("  - {ValidationError}", error);
                }
                _logger.LogError("These validation errors will cause the API to reject the request with a 400 Bad Request.");
                return false;
            }

            _logger.LogInformation("DTO validation passed - all required fields are present and correctly formatted.");

            var jsonContent = JsonSerializer.Serialize(createDto, GetJsonOptions());
            
            _logger.LogInformation("=== JSON PAYLOAD ANALYSIS ===");
            _logger.LogInformation("JSON payload length: {Length} characters", jsonContent.Length);
            _logger.LogInformation("Complete JSON being sent to API: {JsonContent}", jsonContent);
            
            try
            {
                var verificationObject = JsonSerializer.Deserialize<JsonElement>(jsonContent);
                _logger.LogInformation("JSON verification: Well-formed JSON confirmed");
            }
            catch (Exception jsonEx)
            {
                _logger.LogError(jsonEx, "JSON malformation detected - this will cause API communication to fail");
                return false;
            }

            var content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");
            
            _logger.LogInformation("=== HTTP REQUEST ANALYSIS ===");
            _logger.LogInformation("Request URL: {BaseUrl}/api/events", _httpClient.BaseAddress);
            _logger.LogInformation("Request Method: POST");
            _logger.LogInformation("Content-Type: application/json");
            _logger.LogInformation("Request body size: {Size} bytes", System.Text.Encoding.UTF8.GetByteCount(jsonContent));
            
            _logger.LogInformation("Request headers:");
            foreach (var header in _httpClient.DefaultRequestHeaders)
            {
                var headerValue = header.Key.Equals("x-api-key", StringComparison.OrdinalIgnoreCase) 
                    ? "[REDACTED FOR SECURITY]" 
                    : string.Join(", ", header.Value);
                _logger.LogInformation("  - {HeaderName}: {HeaderValue}", header.Key, headerValue);
            }

            var response = await _httpClient.PostAsync("api/events", content);

            _logger.LogInformation("=== API RESPONSE ANALYSIS ===");
            _logger.LogInformation("Response status code: {StatusCode} ({StatusCodeNumber})", response.StatusCode, (int)response.StatusCode);
            _logger.LogInformation("Response reason phrase: {ReasonPhrase}", response.ReasonPhrase);
            
            _logger.LogInformation("Response headers:");
            foreach (var header in response.Headers)
            {
                _logger.LogInformation("  - {HeaderName}: {HeaderValue}", header.Key, string.Join(", ", header.Value));
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            _logger.LogInformation("Response body: {ResponseContent}", responseContent);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("✅ SUCCESS: Event created successfully!");
                _logger.LogInformation("=== EVENT CREATION DIAGNOSTIC END ===");
                return true;
            }
            else
            {
                _logger.LogError("❌ FAILURE: API rejected the request");
                
                if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    _logger.LogError("DIAGNOSIS: 400 Bad Request indicates validation errors or malformed data");
                    if (responseContent.Contains("validation", StringComparison.OrdinalIgnoreCase))
                    {
                        _logger.LogError("SPECIFIC ISSUE: Server-side validation failed. Check the response body for specific field errors.");
                    }
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    _logger.LogError("DIAGNOSIS: 401 Unauthorized indicates API key issues");
                    _logger.LogError("SOLUTION: Verify that your AdminApiKey is correctly configured and has the right permissions");
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                {
                    _logger.LogError("DIAGNOSIS: 403 Forbidden indicates insufficient permissions");
                    _logger.LogError("SOLUTION: Ensure your API key has AdminApiKeyPolicy permissions");
                }
                
                _logger.LogInformation("=== EVENT CREATION DIAGNOSTIC END ===");
                return false;
            }
        }
        catch (HttpRequestException httpEx)
        {
            _logger.LogError(httpEx, "❌ NETWORK ERROR: Cannot reach the API server");
            _logger.LogError("DIAGNOSIS: This suggests connectivity issues, API server downtime, or incorrect base URL");
            await _errorHandlingService.LogErrorAsync($"Network error creating event {eventModel.EventName}", httpEx);
            return false;
        }
        catch (JsonException jsonEx)
        {
            _logger.LogError(jsonEx, "❌ SERIALIZATION ERROR: Cannot convert data to JSON");
            _logger.LogError("DIAGNOSIS: There's a data type issue in our DTO or serialization settings");
            await _errorHandlingService.LogErrorAsync($"JSON error creating event {eventModel.EventName}", jsonEx);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ UNEXPECTED ERROR: Something else went wrong");
            await _errorHandlingService.LogErrorAsync($"Error creating event {eventModel.EventName}", ex);
            return false;
        }
    }

    private async Task AnalyzeCategoryMappingAsync()
    {
        try
        {
            _logger.LogInformation("=== CATEGORY MAPPING ANALYSIS ===");
            _logger.LogInformation("Attempting to retrieve categories to understand the mapping issue...");
            
            var categories = await GetAllCategoriesAsync();
            var categoryList = categories.ToList();
            
            _logger.LogInformation("Found {Count} categories in the system:", categoryList.Count);
            
            foreach (var category in categoryList)
            {
                _logger.LogInformation("  - Category: ID='{CategoryId}', Name='{Name}'", 
                    category.CategoryId, category.Name);
            }
            
            if (categoryList.Any())
            {
                _logger.LogInformation("SOLUTION: You need to map category names to their IDs. For example:");
                _logger.LogInformation("  - If user selects 'music', convert it to the corresponding CategoryId");
                _logger.LogInformation("  - Consider creating a lookup dictionary or modifying your form to send IDs instead of names");
            }
            else
            {
                _logger.LogWarning("No categories found - this might indicate an issue with category retrieval");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to analyze category mapping");
        }
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
            _logger.LogInformation("Categories API returned content: {JsonContent}", jsonContent);

            var categoryDtos = JsonSerializer.Deserialize<List<CategoryDto>>(jsonContent, GetJsonOptions())
                              ?? new List<CategoryDto>();

            _logger.LogInformation("Successfully retrieved {Count} categories", categoryDtos.Count);

            return categoryDtos.Select(dto => _mappingService.MapToCategory(dto));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred while fetching categories");
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