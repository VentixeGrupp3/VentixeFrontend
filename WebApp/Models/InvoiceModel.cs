using System.Text.Json.Serialization;

namespace WebApp.Models
{
    public class InvoiceModel
    {
        public string InvoiceId { get; set; } = "";
        public string BookingId { get; set; } = "";
        public string EventId { get; set; } = "";
        public string UserId { get; set; } = "";

        // billing snapshot
        public string UserName { get; set; } = "";
        public string UserEmail { get; set; } = "";
        public string? UserAddress { get; set; }
        public string? UserPhone { get; set; }

        // event snapshot
        public string EventName { get; set; } = "";
        public string EventOwnerName { get; set; } = "";
        public string EventOwnerEmail { get; set; } = "";
        public string EventOwnerAddress { get; set; } = "";
        public string EventOwnerPhone { get; set; } = "";

        // status & dates
        public bool InvoicePaid { get; set; }
        public DateTime IssuedDate { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime Created { get; set; }

        // totals
        public decimal Subtotal { get; set; }
        public decimal Tax { get; set; }
        public decimal Fee { get; set; }
        public decimal Total { get; set; }

        // line-items
        // The API returns its line-items in a JSON array named "items",
        // so we tell System.Text.Json to bind "items" → this property.
        [JsonPropertyName("items")]
        public List<InvoiceItemModel> InvoiceItems { get; set; }
            = new List<InvoiceItemModel>();
    }
}
