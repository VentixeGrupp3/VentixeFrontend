using System.ComponentModel.DataAnnotations;

namespace WebApp.ViewModels
{
    public class UpdateInvoiceViewModel
    {
        [Required]
        public string InvoiceId { get; set; } = "";

        [Required, Display(Name = "User ID")]
        public string UserId { get; set; } = "";

        [Required, Display(Name = "Booking ID")]
        public string BookingId { get; set; } = "";

        [Required, Display(Name = "Event ID")]
        public string EventId { get; set; } = "";

        [Required, Display(Name = "Recipient Name")]
        public string UserName { get; set; } = "";

        [Required, EmailAddress, Display(Name = "Recipient Email")]
        public string UserEmail { get; set; } = "";

        [Display(Name = "Recipient Address")]
        public string? UserAddress { get; set; }

        [Display(Name = "Recipient Phone")]
        public string? UserPhone { get; set; }

        [Required, Display(Name = "Event Name")]
        public string EventName { get; set; } = "";

        [Required, Display(Name = "Event Owner")]
        public string EventOwnerName { get; set; } = "";

        [Required, EmailAddress, Display(Name = "Owner Email")]
        public string EventOwnerEmail { get; set; } = "";

        [Required, Display(Name = "Owner Address")]
        public string EventOwnerAddress { get; set; } = "";

        [Required, Display(Name = "Owner Phone")]
        public string EventOwnerPhone { get; set; } = "";

        [Display(Name = "Mark as Paid")]
        public bool InvoicePaid { get; set; }

        [Display(Name = "Custom Fee (SEK)")]
        public decimal? CustomFee { get; set; }

        [Display(Name = "Custom Tax Rate (%)")]
        public decimal? CustomTaxRate { get; set; }

        [Required, MinLength(5), Display(Name = "Adjusted By")]
        public string AdjustedBy { get; set; } = "";

        [Required, MinLength(5), Display(Name = "Reason for Change")]
        public string AdjustmentReason { get; set; } = "";

        [Required, MinLength(1), Display(Name = "Line Items")]
        public List<InvoiceItemViewModel> InvoiceItems { get; set; }
            = new List<InvoiceItemViewModel>();
    }
}
