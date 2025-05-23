namespace EventsWebApp.Models.Domain;


public class Event
{
    public string EventId { get; set; } = string.Empty;
    public string EventName { get; set; } = string.Empty;
    public string CategoryId { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string OwnerId { get; set; } = string.Empty;
    public string OwnerName { get; set; } = string.Empty;
    public string OwnerEmail { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string? VenueName { get; set; }
    public string EventDate { get; set; } = string.Empty;
    public string EventTime { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public int TicketsSold { get; set; }
    public string Status { get; set; } = "Draft";
    public List<TicketCategory> TicketCategories { get; set; } = new List<TicketCategory>();

   
    public DateTime GetEventDateTime()
    {
        if (DateTime.TryParse($"{EventDate} {EventTime}", out DateTime result))
        {
            return result;
        }
        return DateTime.MinValue; // Fallback for invalid dates
    }

    public bool HasAvailableTickets => Capacity <= 0 || TicketsSold < Capacity;

    public double TicketsSoldPercentage 
    {
        get
        {
            if (Capacity <= 0) return 0; // Unlimited capacity
            return Math.Round((double)TicketsSold / Capacity * 100, 1);
        }
    }
}