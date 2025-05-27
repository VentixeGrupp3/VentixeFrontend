using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApp.Models;
using WebApp.ViewModels;
using static WebApp.Services.InvoiceApiService;

namespace WebApp.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminInvoicesController : Controller
    {
        private readonly IInvoiceApiClient _api;
        public AdminInvoicesController(IInvoiceApiClient api) => _api = api;

        public async Task<IActionResult> Index(string? selectedId)
        {
            var invoices = (await _api.GetAllInvoicesAsync()).ToList();

            var vm = new InvoicesPageViewModel
            {
                Invoices = invoices,
                SelectedInvoice = invoices
                                      .FirstOrDefault(i => i.InvoiceId == selectedId)
                                  ?? invoices.FirstOrDefault()
            };
            return View(vm);
        }
        public async Task<IActionResult> Download(string id)
        {
            var pdf = await _api.AdminDownloadInvoicePdfAsync(id);
            return File(
                pdf,
                contentType: "application/pdf",
                fileDownloadName: $"invoice_{id}.pdf"
            );
        }
        [HttpGet]
        public IActionResult NewInvoice()
        {
            // start with an empty invoice
            var vm = new CreateManualInvoiceViewModel
            {
                // copy defaults from your InvoiceModel if you like…
                InvoiceItems = new List<InvoiceItemViewModel> { new InvoiceItemViewModel() }
            };
            return View(vm);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveInvoice(CreateManualInvoiceViewModel vm)
        {
            if (!ModelState.IsValid)
                return View("NewInvoice", vm);

            var created = await _api.AdminCreateInvoiceAsync(vm);
            return RedirectToAction("Index", new { selectedId = created.InvoiceId });
        }
        [HttpGet]
        public async Task<IActionResult> ChangeInvoice(string id)
        {
            // 1) fetch the invoice by ID
            var inv = await _api.GetInvoiceByIdAsync(id);

            // 2) map it into your VM
            var vm = new UpdateInvoiceViewModel
            {
                InvoiceId = inv.InvoiceId,
                UserId = inv.UserId,
                BookingId = inv.BookingId,
                EventId = inv.EventId,
                UserName = inv.UserName,
                UserEmail = inv.UserEmail,
                UserAddress = inv.UserAddress,
                UserPhone = inv.UserPhone,
                EventName = inv.EventName,
                EventOwnerName = inv.EventOwnerName,
                EventOwnerEmail = inv.EventOwnerEmail,
                EventOwnerAddress = inv.EventOwnerAddress,
                EventOwnerPhone = inv.EventOwnerPhone,
                InvoicePaid = inv.InvoicePaid,
                CustomFee = inv.Fee,
                CustomTaxRate = (decimal?).25,
                AdjustedBy = "",              // let admin fill
                AdjustmentReason = "",
                InvoiceItems = inv.InvoiceItems
                    .Select(i => new InvoiceItemViewModel
                    {
                        TicketCategory = i.TicketCategory,
                        Quantity = i.Quantity,
                        Price = i.Price
                    }).ToList()
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveChangedInvoice(UpdateInvoiceViewModel vm)
        {
            if (!ModelState.IsValid)
                return View("ChangeInvoice", vm);

            var updated = await _api.AdminUpdateInvoiceAsync(vm);
            return RedirectToAction("Index", new { selectedId = updated.InvoiceId });
        }
        [HttpGet]
        public async Task<IActionResult> SoftDeleteInvoice(string id)
        {
            var inv = await _api.GetInvoiceByIdAsync(id);

            var vm = new DeleteInvoiceViewModel
            {
                InvoiceId = inv.InvoiceId,
                UserId = inv.UserId,
                BookingId = inv.BookingId,
                EventId = inv.EventId,
                UserName = inv.UserName,
                EventName = inv.EventName,
                Total = inv.Total
            };
            return View("SoftDeleteInvoice", vm);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmSoftDelete(DeleteInvoiceViewModel vm)
        {
            if (!ModelState.IsValid)
                return View("SoftDeleteInvoice", vm);

            await _api.SoftDeleteInvoiceAsync(vm);
            return RedirectToAction("Index");
        }
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> HardDeleteInvoice(string invoiceId)
        {
            await _api.HardDeleteInvoiceAsync(invoiceId);
            return RedirectToAction("Index");
        }
    }
}