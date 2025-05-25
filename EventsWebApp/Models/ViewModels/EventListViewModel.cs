using EventsWebApp.Models.ViewModels;
using EventsWebApp.ViewModels;

public class EventListViewModel
{
    public string EventId { get; set; } = string.Empty;
    public string EventName { get; set; } = string.Empty;
    public string EventCategory { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string VenueName { get; set; } = string.Empty;
    public string? EventDate { get; set; }
    public string? EventTime { get; set; }

    public int TicketsSold { get; set; }
    public int Capacity { get; set; }
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

    public decimal? StartingPrice
    {
        get
        {
            if (TicketCategories.Count == 0) return null;
            return TicketCategories.Min(tc => tc.Price);
        }
    }
}