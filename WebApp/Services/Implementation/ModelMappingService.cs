using WebApp.Models.Domain;
using WebApp.Models.DTOs;
using WebApp.Services.Interfaces;
using WebApp.ViewModels;

namespace WebApp.Services.Implementation;

public class ModelMappingService(ILogger<ModelMappingService> logger) : IModelMappingService
{
    private readonly ILogger<ModelMappingService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public Event MapToEvent(EventDto dto)
    {
        if (dto == null)
        {
            _logger.LogWarning("Attempted to map null EventDto");
            return new Event();
        }

        try
        {
            var eventDate = dto.Date != default ? dto.Date.ToString("yyyy-MM-dd") : DateTime.Now.ToString("yyyy-MM-dd");
            var eventTime = dto.Date != default ? dto.Date.ToString("HH:mm") : "00:00";

            _logger.LogInformation("Mapping Event - DTO Date: {DtoDate}, Extracted Date: {EventDate}, Extracted Time: {EventTime}", 
                                  dto.Date, eventDate, eventTime);

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
                EventDate = eventDate,
                EventTime = eventTime,
                Capacity = Math.Max(0, dto.Capacity),
                TicketsSold = Math.Max(0, dto.TicketsSold),
                Status = dto.Status ?? "Draft",
                TicketCategories = []
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error mapping EventDto to Event domain model");
            return new Event();
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
                EventCategory = category?.Name ?? GetCategoryName(domainModel.CategoryId) ?? "Uncategorized",
                Description = domainModel.Description ?? string.Empty,
                Location = domainModel.Location,
                VenueName = domainModel.VenueName ?? string.Empty,
                EventDate = domainModel.EventDate,
                EventTime = domainModel.EventTime,
                TicketsSold = domainModel.TicketsSold,
                Capacity = domainModel.Capacity,
                TicketCategories = domainModel.TicketCategories?
                    .Select(MapToTicketCategoryViewModel)
                    .ToList() ?? new List<TicketCategoryViewModel>()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error mapping Event to EventListViewModel");
            return new EventListViewModel();
        }
    }

    private static string? GetCategoryName(string categoryId)
    {
        return categoryId switch
        {
            "1" => "Music",
            "2" => "Technology", 
            "3" => "Health & Wellness",
            "4" => "Food & Culinary",
            "5" => "Art & Design",
            "6" => "Fashion",
            "7" => "Outdoor & Adventure",
            _ => null
        };
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
            _logger.LogInformation("Mapping to EventFormViewModel - EventTime: '{EventTime}', EventDate: '{EventDate}'", 
                                  domainModel.EventTime, domainModel.EventDate);

            return new EventFormViewModel
            {
                EventId = domainModel.EventId,
                EventName = domainModel.EventName,
                EventCategory = domainModel.CategoryId,
                Description = domainModel.Description,
                EventDate = domainModel.EventDate,
                EventTime = domainModel.EventTime,
                Location = domainModel.Location,
                VenueName = domainModel.VenueName ?? string.Empty,
                Capacity = domainModel.Capacity,
                TicketCategories = domainModel.TicketCategories?
                    .Select(MapToTicketCategoryViewModel)
                    .ToList() ?? []
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
            var eventDateTime = domainModel.GetEventDateTime();

            return new EventCardViewModel
            {
                EventId = domainModel.EventId,
                EventName = domainModel.EventName,
                Description = domainModel.Description ?? string.Empty,
                EventCategory = string.Empty,
                EventDate = eventDateTime.Date,
                EventTime = eventDateTime,
                Location = domainModel.Location,
                Capacity = domainModel.Capacity,
                TicketsSold = domainModel.TicketsSold,
                TicketCategories = domainModel.TicketCategories?
                    .Select(MapToTicketCategoryViewModel)
                    .ToList() ?? []
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
                CategoryId = dto.Id.ToString(),
                Name = dto.Name ?? string.Empty,
                Description = dto.Description,
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
                Category = dto.TicketCategory ?? string.Empty,
                Price = Math.Max(0, dto.Price),
                AvailableQuantity = dto.AvailableQuantity,
                Description = dto.Description ?? string.Empty,
                IsAvailable = dto.IsPurchasable()
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
            return [];
        }

        try
        {
            var categoryLookup = (categories ?? Enumerable.Empty<Category>())
                .Where(c => !string.IsNullOrWhiteSpace(c.CategoryId))
                .ToDictionary(c => c.CategoryId, c => c);

            return events.Select(evt => 
            {
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

    private TicketCategoryViewModel MapToTicketCategoryViewModel(TicketCategory domainModel)
    {
        if (domainModel == null)
        {
            return new TicketCategoryViewModel();
        }

        try
        {
            return new TicketCategoryViewModel
            {
                TicketId = domainModel.TicketId,
                TicketCategory = domainModel.Category,
                Price = domainModel.Price,
                AvailableQuantity = domainModel.AvailableQuantity,
                Description = domainModel.Description
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error mapping TicketCategory to TicketCategoryViewModel");
            return new TicketCategoryViewModel();
        }
    }
}