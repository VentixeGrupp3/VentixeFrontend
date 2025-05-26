using EventsWebApp.Models.Domain;
using EventsWebApp.Models.DTOs;
using EventsWebApp.Models.ViewModels;

namespace EventsWebApp.Services.Interfaces;

public interface IModelMappingService
{
    Event MapToEvent(EventDto dto);
    EventDto MapToEventDto(Event domainModel);
    EventListViewModel MapToEventListViewModel(Event domainModel, Category? category = null);
    EventFormViewModel MapToEventFormViewModel(Event domainModel);
    EventCardViewModel MapToEventCardViewModel(Event domainModel);
    Category MapToCategory(CategoryDto dto);
    TicketCategory MapToTicketCategory(TicketCategoryDto dto);
    IEnumerable<EventListViewModel> MapToEventListViewModels(
        IEnumerable<Event> events, 
        IEnumerable<Category> categories);
}
