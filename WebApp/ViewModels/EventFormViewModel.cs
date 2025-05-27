using System.ComponentModel.DataAnnotations;

namespace WebApp.ViewModels;

public class EventFormViewModel
{
    public string EventId { get; set; } = string.Empty;
    public string EventName { get; set; } = string.Empty;
    public string EventCategory { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string OwnerName { get; set; } = string.Empty;
    public string OwnerEmail { get; set; } = string.Empty;
    public string OwnerAddress { get; set; } = string.Empty;
    public string OwnerPhone { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    [Required(ErrorMessage = "Venue name is required")]
    [StringLength(200, ErrorMessage = "Venue name cannot exceed 200 characters")]
    [Display(Name = "Venue Name")]
    public string VenueName { get; set; } = string.Empty;
    public string EventDate { get; set; } = string.Empty;
    public string EventTime { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public int TicketsSold { get; set; }
    public string Status { get; set; } = string.Empty;
    public List<TicketCategoryViewModel> TicketCategories { get; set; } = new();
    
    public double TicketsSoldPercentage
    {
        get
        {
            if (Capacity <= 0) return 0; // Unlimited capacity events
            return Math.Round((double)TicketsSold / Capacity * 100, 1);
        }
    }
    
    public bool HasAvailableTickets => Capacity <= 0 || TicketsSold < Capacity;
    
    public DateTime? ParsedEventDateTime
    {
        get
        {
            if (DateTime.TryParse($"{EventDate} {EventTime}", out DateTime result))
                return result;
            return null;
        }
    }
    
    public bool IsUpcoming => ParsedEventDateTime.HasValue && ParsedEventDateTime > DateTime.Now;
    
    public string FormattedEventDateTime
    {
        get
        {
            if (ParsedEventDateTime.HasValue)
                return ParsedEventDateTime.Value.ToString("MMM dd, yyyy 'at' h:mm tt");
            return "Date/time not available";
        }
    }
    
    public string FormattedOwnerContact
    {
        get
        {
            var parts = new List<string>();
            if (!string.IsNullOrWhiteSpace(OwnerEmail)) parts.Add(OwnerEmail);
            if (!string.IsNullOrWhiteSpace(OwnerPhone)) parts.Add(OwnerPhone);
            return string.Join(" | ", parts);
        }
    }
    
    public decimal? StartingPrice
    {
        get
        {
            if (TicketCategories.Count == 0) return null;
            return TicketCategories.Where(tc => tc.Price > 0).Min(tc => tc.Price);
        }
    }
}