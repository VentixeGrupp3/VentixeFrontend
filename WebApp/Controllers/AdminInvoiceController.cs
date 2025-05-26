using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
    }
}