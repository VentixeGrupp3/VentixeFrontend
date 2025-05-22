namespace EventsWebApp.ViewModels;

public class EventCardViewModel
{
public string EventId { get; set; } = string.Empty;
public string EventName { get; set; } = string.Empty;
public string Description { get; set; } = string.Empty;
public string EventCategory { get; set; } = string.Empty;
public DateTime EventDate { get; set; }
public TimeSpan EventTime { get; set; }
public string Location { get; set; } = string.Empty;
public int Capacity { get; set; }
public int TicketsSold { get; set; }
public List<TicketCategoryViewModel> TicketCategories { get; set; } = new();
}