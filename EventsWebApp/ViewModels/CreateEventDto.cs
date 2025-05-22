using System.ComponentModel.DataAnnotations;

namespace EventsWebApp.ViewModels;

public class CreateEventDto
{
    [Required, StringLength(200)]
    public string EventName { get; set; }

    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Category ID cannot be 0.")]
    public string CategoryId { get; set; } = null!;

    [StringLength(2000)]
    public string Description { get; set; }

    [Required, StringLength(100)]
    public string OwnerId { get; set; }

    [Required, StringLength(200)]
    public string OwnerName { get; set; }

    [Required, EmailAddress, StringLength(256)]
    public string OwnerEmail { get; set; }

    [Required, StringLength(200)]
    public string Location { get; set; }

    [Required, StringLength(200)]
    public string VenueName { get; set; }

    [Required]
    [RegularExpression(@"^\d{4}-\d{2}-\d{2}$", ErrorMessage = "Date must be in format YYYY-MM-DD")]
    public string EventDate { get; set; }

    [Required]
    [RegularExpression(@"^([01]\d|2[0-3]):([0-5]\d)$", ErrorMessage = "Time must be in 24-hour format HH:MM")]
    public string EventTime { get; set; }

    [Range(0, int.MaxValue)]
    public int Capacity { get; set; }

    public string Status { get; set; } = "Draft";
}
