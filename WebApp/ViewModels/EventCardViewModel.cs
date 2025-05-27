namespace WebApp.ViewModels;
public class EventCardViewModel
{
    public string EventId { get; set; } = string.Empty;
    public string EventName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string EventCategory { get; set; } = string.Empty;
    public DateTime EventDate { get; set; }
    public DateTime EventTime { get; set; }
    public string Location { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public int TicketsSold { get; set; }
    public List<TicketCategoryViewModel> TicketCategories { get; set; } = new();
    
    public DateTime FullEventDateTime 
    { 
        get 
        {
            if (EventTime.TimeOfDay != TimeSpan.Zero)
            {
                return EventDate.Date.Add(EventTime.TimeOfDay);
            }
            return EventDate;
        }
    }
    
    // Show events that haven't ended yet (with small buffer for current events)
    public bool IsUpcoming => FullEventDateTime > DateTime.Now.AddHours(-3);
    
    public int DaysUntilEvent
    {
        get
        {
            var timeUntil = FullEventDateTime - DateTime.Now;
            return Math.Max(0, (int)timeUntil.TotalDays);
        }
    }
    
    public string RelativeTimeDescription
    {
        get
        {
            var now = DateTime.Now;
            var eventDateTime = FullEventDateTime;
            
            if (eventDateTime <= now.AddHours(-3))
            {
                return "Past Event";
            }
            
            if (eventDateTime <= now.AddHours(3))
            {
                return "Happening Soon";
            }
            
            var daysUntil = DaysUntilEvent;
            return daysUntil switch
            {
                0 => "Today",
                1 => "Tomorrow",
                < 7 => $"In {daysUntil} days",
                < 14 => "Next week",
                < 30 => $"In {daysUntil / 7} weeks",
                _ => "More than a month away"
            };
        }
    }
}