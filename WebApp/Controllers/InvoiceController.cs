using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApp.Services;
using WebApp.ViewModels;

namespace WebApp.Controllers
{
    [Authorize(Roles = "User")]
    public class InvoicesController : Controller
    {
        private readonly InvoiceApiService.IInvoiceApiClient _api;
        public InvoicesController(InvoiceApiService.IInvoiceApiClient api) => _api = api;

        private string GetCurrentUserId()
            => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        public async Task<IActionResult> Index(string? selectedId)
        {
            var userId = GetCurrentUserId();
            var invoices = (await _api.GetMyInvoicesAsync(userId)).ToList();
            var vm = new InvoicesPageViewModel
            {
                Invoices = invoices,
                SelectedInvoice = invoices
                                      .FirstOrDefault(i => i.InvoiceId == selectedId)
                                  ?? invoices.FirstOrDefault()
            };
            return View(vm);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Pay(string id)
        {
            var userId = GetCurrentUserId();
            await _api.PayInvoiceAsync(userId, id);
            return RedirectToAction("Index", new { selectedId = id });
        }

        public async Task<IActionResult> UserDownload(string id)
        {
            var userId = GetCurrentUserId();
            var pdf = await _api.DownloadInvoicePdfAsync(userId, id);
            return File(pdf, "application/pdf", $"invoice_{id}.pdf");
        }
    }
}
