namespace EventsWebApp.Models;

public class EventDetailsDto
{
    public string EventId { get; set; }
    public string EventName { get; set; }
    public string EventCategory { get; set; }
    public string Description { get; set; }
    public string Location { get; set; }
    public string VenueName { get; set; }
    public string EventDate { get; set; }
    public string EventTime { get; set; }
    public int Capacity { get; set; }
    public int TicketsSold { get; set; }
    public string Status { get; set; }
    public List<TicketCategoryDto> TicketCategories { get; set; } = new List<TicketCategoryDto>();
}
