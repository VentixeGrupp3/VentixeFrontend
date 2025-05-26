using WebApp.Models;

namespace WebApp.ViewModels
{
    public class InvoicesPageViewModel
    {
        public List<InvoiceModel> Invoices { get; set; } = new();

        // summary counts
        public int TotalInvoices => Invoices.Count;
        public int PaidCount => Invoices.Count(i => i.InvoicePaid);
        public int UnpaidCount => Invoices.Count(i => !i.InvoicePaid);

        // the one that’s selected in the right-hand pane
        public InvoiceModel? SelectedInvoice { get; set; }
    }
}
