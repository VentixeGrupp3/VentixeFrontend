using EventsWebApp.Models.Domain;
using EventsWebApp.Models.DTOs;
using EventsWebApp.Models.ViewModels;

namespace EventsWebApp.Services.Interfaces;

public interface IEventsApiService
{
    Task<IEnumerable<Event>> GetAllEventsAsync();
    Task<Event?> GetEventByIdAsync(string eventId);
    Task<bool> CreateEventAsync(Event eventModel);
    Task<bool> UpdateEventAsync(string eventId, Event eventModel);
    Task<bool> DeleteEventAsync(string eventId);
    Task<IEnumerable<Category>> GetAllCategoriesAsync();
    Task<bool> IsApiHealthyAsync();
    Task<bool> IsAdminAsync();
}