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
    public string OwnerAddress { get; set; } = string.Empty;
    public string OwnerPhone { get; set; } = string.Empty;
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
    
        if (!string.IsNullOrWhiteSpace(EventDate) && !string.IsNullOrWhiteSpace(EventTime))
        {
            var combinedString = $"{EventDate} {EventTime}";
            if (DateTime.TryParse(combinedString, out DateTime combined))
            {
                return combined;
            }
        }
    
        if (!string.IsNullOrWhiteSpace(EventDate))
        {
            if (DateTime.TryParse(EventDate, out DateTime dateOnly))
            {
                if (!string.IsNullOrWhiteSpace(EventTime) && TimeSpan.TryParse(EventTime, out TimeSpan timeSpan))
                {
                    return dateOnly.Add(timeSpan);
                }
            
                return dateOnly.AddHours(12);
            }
        }
    
        return DateTime.Now.AddDays(1);
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

    public bool HasCompleteOwnerInformation()
    {
        return !string.IsNullOrWhiteSpace(OwnerName) && 
               !string.IsNullOrWhiteSpace(OwnerEmail) && 
               !string.IsNullOrWhiteSpace(OwnerAddress) && 
               !string.IsNullOrWhiteSpace(OwnerPhone);
    }

    public string GetFormattedOwnerContact()
    {
        var contactParts = new List<string>();
        
        if (!string.IsNullOrWhiteSpace(OwnerName))
            contactParts.Add($"Name: {OwnerName}");
        
        if (!string.IsNullOrWhiteSpace(OwnerEmail))
            contactParts.Add($"Email: {OwnerEmail}");
        
        if (!string.IsNullOrWhiteSpace(OwnerPhone))
            contactParts.Add($"Phone: {OwnerPhone}");
        
        if (!string.IsNullOrWhiteSpace(OwnerAddress))
            contactParts.Add($"Address: {OwnerAddress}");
        
        return string.Join(" | ", contactParts);
    }
}