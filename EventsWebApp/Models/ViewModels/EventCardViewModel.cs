using EventsWebApp.ViewModels;

namespace EventsWebApp.Models.ViewModels;
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
    public DateTime FullEventDateTime => EventDate.Add(EventTime.TimeOfDay);
    public bool IsUpcoming => FullEventDateTime > DateTime.Now;
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
