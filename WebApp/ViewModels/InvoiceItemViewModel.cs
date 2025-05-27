using System.ComponentModel.DataAnnotations;

namespace WebApp.ViewModels
{
    public class InvoiceItemViewModel
    {
        [Required, Display(Name = "Ticket Category")]
        public string TicketCategory { get; set; } = "";

        [Required, Range(1, int.MaxValue), Display(Name = "Quantity")]
        public int Quantity { get; set; }

        [Required, Range(0, double.MaxValue), Display(Name = "Unit Price")]
        public decimal Price { get; set; }
    }
}
