public class UpdateEventDto
{
    public string? EventName { get; set; }
    public string? EventCategory { get; set; }
    public string? Description { get; set; }
    public string? OwnerId { get; set; }
    public string? OwnerName { get; set; }
    public string? OwnerEmail { get; set; }
    public string? Location { get; set; }
    public string? VenueName { get; set; }
    public string EventDate { get; set; } = null!;
    public string EventTime { get; set; } = null!;
    public int? Capacity { get; set; }
    public string? Status { get; set; }
}