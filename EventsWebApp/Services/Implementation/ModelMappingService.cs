using EventsWebApp.Models.Domain;
using EventsWebApp.Models.DTOs;
using EventsWebApp.Models.ViewModels;
using EventsWebApp.Services.Interfaces;
using EventsWebApp.ViewModels;

namespace EventsWebApp.Services.Implementation;

public class ModelMappingService(ILogger<ModelMappingService> logger) : IModelMappingService
{
    private readonly ILogger<ModelMappingService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public Event MapToEvent(EventDto dto)
    {
        if (dto == null)
        {
            _logger.LogWarning("Attempted to map null EventDto");
            return new Event(); // Return empty event rather than throwing
        }

        try
        {
            return new Event
            {
                EventId = dto.EventId ?? string.Empty,
                EventName = dto.Title ?? string.Empty,
                CategoryId = dto.CategoryId ?? string.Empty,
                Description = dto.Description,
                OwnerId = dto.OwnerId ?? string.Empty,
                OwnerName = dto.OwnerName ?? string.Empty,
                OwnerEmail = dto.OwnerEmail ?? string.Empty,
                Location = dto.Location ?? string.Empty,
                VenueName = dto.VenueName,
                EventDate = dto.GetDateString(),
                EventTime = dto.GetTimeString(),
                Capacity = Math.Max(0, dto.Capacity),
                TicketsSold = Math.Max(0, dto.TicketsSold),
                Status = dto.Status ?? "Draft",
                TicketCategories = new List<TicketCategory>() // Will be populated separately if needed
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error mapping EventDto to Event domain model");
            return new Event(); // Return empty event rather than crashing
        }
    }
    public EventDto MapToEventDto(Event domainModel)
    {
        if (domainModel == null)
        {
            _logger.LogWarning("Attempted to map null Event domain model");
            return new EventDto();
        }

        try
        {
            // Combine date and time strings back into DateTime for API
            var eventDateTime = domainModel.GetEventDateTime();

            return new EventDto
            {
                EventId = domainModel.EventId,
                Title = domainModel.EventName,
                CategoryId = domainModel.CategoryId,
                Description = domainModel.Description,
                OwnerId = domainModel.OwnerId,
                OwnerName = domainModel.OwnerName,
                OwnerEmail = domainModel.OwnerEmail,
                Location = domainModel.Location,
                VenueName = domainModel.VenueName,
                Date = eventDateTime,
                Capacity = domainModel.Capacity,
                TicketsSold = domainModel.TicketsSold,
                Status = domainModel.Status
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error mapping Event domain model to EventDto");
            return new EventDto();
        }
    }
    public EventListViewModel MapToEventListViewModel(Event domainModel, Category? category = null)
    {
        if (domainModel == null)
        {
            _logger.LogWarning("Attempted to map null Event to EventListViewModel");
            return new EventListViewModel();
        }

        try
        {
            return new EventListViewModel
            {
                EventId = domainModel.EventId,
                EventName = domainModel.EventName,
                EventCategory = category?.Name ?? "Uncategorized", // Use category name or fallback
                Description = domainModel.Description ?? string.Empty,
                Location = domainModel.Location,
                VenueName = domainModel.VenueName ?? string.Empty,
                EventDate = domainModel.EventDate,
                EventTime = domainModel.EventTime,
                TicketsSold = domainModel.TicketsSold,
                Capacity = domainModel.Capacity,
                TicketCategories = new List<TicketCategoryViewModel>() // Will add if needed
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error mapping Event to EventListViewModel");
            return new EventListViewModel();
        }
    }

    public EventFormViewModel MapToEventFormViewModel(Event domainModel)
    {
        if (domainModel == null)
        {
            _logger.LogWarning("Attempted to map null Event to EventFormViewModel");
            return new EventFormViewModel();
        }

        try
        {
            return new EventFormViewModel
            {
                EventId = domainModel.EventId,
                EventName = domainModel.EventName,
                EventCategory = domainModel.CategoryId,
                Description = domainModel.Description,
                EventDate = domainModel.EventDate,
                EventTime = domainModel.EventTime,
                Location = domainModel.Location,
                VenueName = domainModel.VenueName,
                Capacity = domainModel.Capacity,
                TicketCategories = domainModel.TicketCategories
                    .Select(tc => MapToTicketCategoryDto(tc))
                    .ToList()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error mapping Event to EventFormViewModel");
            return new EventFormViewModel();
        }
    }

    public EventCardViewModel MapToEventCardViewModel(Event domainModel)
    {
        if (domainModel == null)
        {
            _logger.LogWarning("Attempted to map null Event to EventCardViewModel");
            return new EventCardViewModel();
        }

        try
        {
            // Parse the date and time for proper DateTime/TimeSpan objects
            var eventDateTime = domainModel.GetEventDateTime();

            return new EventCardViewModel
            {
                EventId = domainModel.EventId,
                EventName = domainModel.EventName,
                Description = domainModel.Description ?? string.Empty,
                EventCategory = string.Empty,
                EventDate = eventDateTime.Date,
                EventTime = eventDateTime.TimeOfDay,
                Location = domainModel.Location,
                Capacity = domainModel.Capacity,
                TicketsSold = domainModel.TicketsSold,
                TicketCategories = new List<TicketCategoryViewModel>() // Will be populated if needed
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error mapping Event to EventCardViewModel");
            return new EventCardViewModel();
        }
    }
    public Category MapToCategory(CategoryDto dto)
    {
        if (dto == null)
        {
            _logger.LogWarning("Attempted to map null CategoryDto");
            return new Category();
        }

        try
        {
            return new Category
            {
                CategoryId = dto.CategoryId ?? string.Empty,
                Name = dto.Name ?? string.Empty,
                Description = dto.Description,
                IsActive = true
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error mapping CategoryDto to Category domain model");
            return new Category();
        }
    }

    public TicketCategory MapToTicketCategory(TicketCategoryDto dto)
    {
        if (dto == null)
        {
            _logger.LogWarning("Attempted to map null TicketCategoryDto");
            return new TicketCategory();
        }

        try
        {
            return new TicketCategory
            {
                TicketId = dto.TicketId ?? Guid.NewGuid().ToString(),
                TicketCategory = dto.TicketCategory ?? string.Empty,
                Price = Math.Max(0, dto.Price), // Ensure non-negative price
                AvailableQuantity = dto.AvailableQuantity,
                Description = dto.Description ?? string.Empty,
                IsAvailable = dto.IsPurchasable() // Use DTO's business logic
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error mapping TicketCategoryDto to TicketCategory domain model");
            return new TicketCategory();
        }
    }

    public IEnumerable<EventListViewModel> MapToEventListViewModels(
        IEnumerable<Event> events, 
        IEnumerable<Category> categories)
    {
        if (events == null || !events.Any())
        {
            return Enumerable.Empty<EventListViewModel>();
        }

        try
        {
            // Create lookup dictionary for efficient category finding
            var categoryLookup = (categories ?? Enumerable.Empty<Category>())
                .Where(c => !string.IsNullOrWhiteSpace(c.CategoryId))
                .ToDictionary(c => c.CategoryId, c => c);

            return events.Select(evt => 
            {
                // Find matching category efficiently
                categoryLookup.TryGetValue(evt.CategoryId, out var category);
                return MapToEventListViewModel(evt, category);
            }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error mapping multiple Events to EventListViewModels");
            return Enumerable.Empty<EventListViewModel>();
        }
    }

    private TicketCategoryDto MapToTicketCategoryDto(TicketCategory domainModel)
    {
        if (domainModel == null)
        {
            return new TicketCategoryDto();
        }

        return new TicketCategoryDto
        {
            TicketId = domainModel.TicketId,
            TicketCategory = domainModel.TicketCategory,
            Price = domainModel.Price,
            AvailableQuantity = domainModel.AvailableQuantity,
            Description = domainModel.Description
        };
    }
}