namespace EventsWebApp.Models.DTOs;

public class CreateEventDto
{
    public string EventName { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public string? Description { get; set; }
    public string OwnerId { get; set; } = string.Empty;
    public string OwnerName { get; set; } = string.Empty;
    public string OwnerEmail { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string VenueName { get; set; } = string.Empty;
    public string EventDate { get; set; } = string.Empty;
    public string EventTime { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public string Status { get; set; } = "Draft";
}