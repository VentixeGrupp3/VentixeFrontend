namespace WebApp.Models
{
    public class InvoiceItemModel
    {
        public int Id { get; set; }
        public string TicketCategory { get; set; } = "";
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public decimal Amount => Price * Quantity;
    }
}
