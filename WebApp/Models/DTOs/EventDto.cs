namespace WebApp.Models.DTOs;

public class EventDto
{
    public string EventId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string CategoryId { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string OwnerId { get; set; } = string.Empty;
    public string OwnerName { get; set; } = string.Empty;
    public string OwnerEmail { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string? VenueName { get; set; }
    public DateTime Date { get; set; }
    public int SeatsAvailable { get; set; }
    public int Capacity { get; set; }
    public int TicketsSold { get; set; }
    public string Status { get; set; } = string.Empty;


    public string GetDateString()
    {
        return Date.ToString("yyyy-MM-dd");
    }

    public string GetTimeString()
    {
        return Date.ToString("HH:mm");
    }

    public string GetFormattedDateTime()
    {
        return Date.ToString("MMM dd, yyyy 'at' h:mm tt");
    }
}