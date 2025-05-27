using System.ComponentModel.DataAnnotations;

namespace WebApp.ViewModels
{
    public class CreateManualInvoiceViewModel
    {
        [Required]
        [Display(Name = "Booking ID")]
        public string BookingId { get; set; } = "";

        [Required]
        [Display(Name = "User ID")]
        public string UserId { get; set; } = "";

        [Required]
        [Display(Name = "Event ID")]
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

        [Required]
        public List<InvoiceItemViewModel> InvoiceItems { get; set; }
            = new List<InvoiceItemViewModel> { new InvoiceItemViewModel() };

        [Display(Name = "Custom Fee (SEK)")]
        public decimal? CustomFee { get; set; }

        [Display(Name = "Custom Tax Rate (%)")]
        public decimal? CustomTaxRate { get; set; }
    }
}
