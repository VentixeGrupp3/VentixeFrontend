using EventsWebApp.Models.DTOs;
using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace EventsWebApp.Models.ViewModels;
public class EventFormViewModel
{
    public string? EventId { get; set; }

    [Required(ErrorMessage = "Event name is required")]
    [StringLength(200, ErrorMessage = "Event name cannot exceed 200 characters")]
    [Display(Name = "Event Name")]
    public string EventName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Category is required")]
    [Display(Name = "Category")]
    public string EventCategory { get; set; } = string.Empty;

    [StringLength(2000, ErrorMessage = "Description cannot exceed 2000 characters")]
    [Display(Name = "Description")]
    public string? Description { get; set; }

    [Required(ErrorMessage = "Event date is required")]
    [DataType(DataType.Date)]
    [Display(Name = "Event Date")]
    public string EventDate { get; set; } = string.Empty;

    [Required(ErrorMessage = "Event time is required")]
    [DataType(DataType.Time)]
    [Display(Name = "Event Time")]
    public string EventTime { get; set; } = string.Empty;

    [Required(ErrorMessage = "Location is required")]
    [StringLength(200, ErrorMessage = "Location cannot exceed 200 characters")]
    [Display(Name = "Location")]
    public string Location { get; set; } = string.Empty;

    [StringLength(200, ErrorMessage = "Venue name cannot exceed 200 characters")]
    [Display(Name = "Venue Name")]
    public string? VenueName { get; set; }

    [Required(ErrorMessage = "Capacity is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Capacity must be at least 1")]
    [Display(Name = "Capacity")]
    public int Capacity { get; set; }

    public List<TicketCategoryDto> TicketCategories { get; set; } = new();

    public bool IsValidDate()
    {
        return DateTime.TryParseExact(EventDate, "yyyy-MM-dd", null, DateTimeStyles.None, out _);
    }

    public bool IsValidTime()
    {
        return TimeSpan.TryParseExact(EventTime, @"hh\:mm", null, out _);
    }

    public DateTime? GetEventDateTime()
    {
        if (IsValidDate() && IsValidTime() && 
            DateTime.TryParse($"{EventDate} {EventTime}", out DateTime result))
        {
            return result;
        }
        return null;
    }
}