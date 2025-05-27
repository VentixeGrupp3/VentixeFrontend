using System.ComponentModel.DataAnnotations;

namespace WebApp.ViewModels
{
    public class DeleteInvoiceViewModel
    {
        // read‐only fields you display
        public string InvoiceId { get; set; } = "";
        public string UserId { get; set; } = "";
        public string BookingId { get; set; } = "";
        public string EventId { get; set; } = "";
        public string UserName { get; set; } = "";
        public string EventName { get; set; } = "";
        public decimal Total { get; set; }

        // soft‐delete inputs
        [Required, MinLength(5)]
        [Display(Name = "Deleted By")]
        public string DeletedBy { get; set; } = "";

        [Required, MinLength(5)]
        [Display(Name = "Reason for Deletion")]
        public string DeletionReason { get; set; } = "";
    }
}
