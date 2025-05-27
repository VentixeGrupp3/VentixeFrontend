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
        public IActionResult NewInvoice()
        {
            // start with an empty invoice
            var vm = new InvoiceModel
            {
                IssuedDate = System.DateTime.Today,
                InvoiceItems = new List<InvoiceItemModel> {
                    new InvoiceItemModel()  // one blank row
                }
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
    }
}