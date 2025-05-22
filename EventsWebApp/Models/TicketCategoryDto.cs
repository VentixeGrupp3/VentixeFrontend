namespace EventsWebApp.Models
{
    public class TicketCategoryDto
    {
        public string TicketId { get; set; } = Guid.NewGuid().ToString();
        public string TicketCategory { get; set; }
        public decimal Price { get; set; }
        public int AvailableQuantity { get; set; }
        public string Description { get; set; }
    }
}
