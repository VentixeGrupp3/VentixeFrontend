namespace EventsWebApp.ViewModels;
public class EventViewModel
{
    public string EventId { get; set; }
    public string EventName { get; set; }
    public string CategoryId { get; set; } = null!;
    public string Description { get; set; }
    public string OwnerId { get; set; }
    public string OwnerName { get; set; }
    public string OwnerEmail { get; set; }
    public string Location { get; set; }
    public string VenueName { get; set; }
    public string EventDate { get; set; }
    public string EventTime { get; set; }
    public int Capacity { get; set; }
    public int TicketsSold { get; set; }
    public string Status { get; set; }
}
