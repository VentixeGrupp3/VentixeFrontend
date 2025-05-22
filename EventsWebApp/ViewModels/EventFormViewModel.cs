using EventsWebApp.Models;
using System.ComponentModel.DataAnnotations;

namespace EventsWebApp.ViewModels;

public class EventFormViewModel
{
    public string? EventId { get; set; }

    [Required]
    [StringLength(200)]
    public string EventName { get; set; } = string.Empty;

    [Required]
    public string EventCategory { get; set; } = string.Empty;

    [StringLength(2000)]
    public string? Description { get; set; }

    [Required]
    [DataType(DataType.Date)]
    public string EventDate { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Time)]
    public string EventTime { get; set; } = string.Empty;

    [Required]
    [StringLength(200)]
    public string Location { get; set; } = string.Empty;

    [StringLength(200)]
    public string? VenueName { get; set; }

    [Required]
    [Range(1, int.MaxValue)]
    public int Capacity { get; set; }

    public List<TicketCategoryDto> TicketCategories { get; set; } = new List<TicketCategoryDto>();
}
