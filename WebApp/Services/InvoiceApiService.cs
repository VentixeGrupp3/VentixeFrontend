using System.Net;
using WebApp.Models;
using WebApp.ViewModels;

namespace WebApp.Services
{
    public class InvoiceApiService
    {
        public interface IInvoiceApiClient
        {
            Task<IEnumerable<InvoiceModel>> GetMyInvoicesAsync(string userId);
            Task<InvoiceModel> PayInvoiceAsync(string userId, string invoiceId);
            Task<IEnumerable<InvoiceModel>> GetAllInvoicesAsync();
            Task<byte[]> AdminDownloadInvoicePdfAsync(string invoiceId);
            Task<byte[]> DownloadInvoicePdfAsync(string userId, string invoiceId);
            Task<InvoiceModel> GetInvoiceByIdAsync(string id);
            Task<InvoiceModel> AdminCreateInvoiceAsync(CreateManualInvoiceViewModel vm);
        }

        public class InvoiceApiClient : IInvoiceApiClient
        {
            private readonly HttpClient _http;
            public InvoiceApiClient(HttpClient http) => _http = http;

            public async Task<IEnumerable<InvoiceModel>> GetMyInvoicesAsync(string userId)
            {
                using var req = new HttpRequestMessage(HttpMethod.Get, "Invoices/user-invoices");
                req.Headers.Add("x-user-id", userId);

                using var res = await _http.SendAsync(req);

                // 1) If the API replies 404 (and you return an empty array from the server),
                //    we treat that as “no invoices” rather than an error:
                if (res.StatusCode == HttpStatusCode.NotFound)
                    return Enumerable.Empty<InvoiceModel>();

                // 2) All other non‐2xx statuses bubble up as exceptions:
                res.EnsureSuccessStatusCode();

                // 3) Deserialize; if the body is [] or null, return an empty list
                var invoices = await res.Content.ReadFromJsonAsync<IEnumerable<InvoiceModel>>();
                return invoices ?? Enumerable.Empty<InvoiceModel>();
            }
            public async Task<InvoiceModel> PayInvoiceAsync(string userId, string invoiceId)
            {
                using var req = new HttpRequestMessage(
                    HttpMethod.Put,
                    $"Invoices/user-pay-invoice/{invoiceId}"
                );
                req.Headers.Add("x-user-id", userId);

                using var res = await _http.SendAsync(req);
                res.EnsureSuccessStatusCode();

                // returns the updated invoice (your API returns Ok(result.Invoice))
                return await res.Content.ReadFromJsonAsync<InvoiceModel>()
                       ?? throw new Exception("Empty response from PayInvoice");
            }
            public async Task<IEnumerable<InvoiceModel>> GetAllInvoicesAsync()
            {
                // GET /api/Invoices/admin-get-all-invoices
                using var req = new HttpRequestMessage(HttpMethod.Get, "Invoices/admin-get-all-invoices");
                using var res = await _http.SendAsync(req);
                if (!res.IsSuccessStatusCode)
                {
                    var text = await res.Content.ReadAsStringAsync();
                    throw new Exception($"Invoice API failed ({res.StatusCode}): {text}");
                }
                var invoices = await res.Content.ReadFromJsonAsync<IEnumerable<InvoiceModel>>();
                return invoices ?? Enumerable.Empty<InvoiceModel>();
            }
            public async Task<byte[]> AdminDownloadInvoicePdfAsync(string invoiceId)
            {
                // Because this client will be the AdminClient (in your Admin controller),
                // we call the admin‐only endpoint:
                var url = $"Invoices/admin-download-invoice/{invoiceId}/pdf";

                using var req = new HttpRequestMessage(HttpMethod.Get, url);
                using var res = await _http.SendAsync(req);

                if (!res.IsSuccessStatusCode)
                {
                    var text = await res.Content.ReadAsStringAsync();
                    throw new Exception($"Invoice API failed ({res.StatusCode}): {text}");
                }

                return await res.Content.ReadAsByteArrayAsync();
            }
            public async Task<byte[]> DownloadInvoicePdfAsync(string userId, string invoiceId)
            {
                var url = $"Invoices/user-download-invoice/{invoiceId}/pdf";
                using var req = new HttpRequestMessage(HttpMethod.Get, url);

                // add the required x-user-id header
                req.Headers.Add("x-user-id", userId);

                using var res = await _http.SendAsync(req);
                if (!res.IsSuccessStatusCode)
                {
                    var text = await res.Content.ReadAsStringAsync();
                    throw new Exception($"Invoice API failed ({res.StatusCode}): {text}");
                }
                return await res.Content.ReadAsByteArrayAsync();
            }
            
            public async Task<InvoiceModel> GetInvoiceByIdAsync(string id)
            {
                using var res = await _http.GetAsync($"Invoices/admin-get-user-invoice/{id}");
                res.EnsureSuccessStatusCode();
                return await res.Content.ReadFromJsonAsync<InvoiceModel>();
            }
            public async Task<InvoiceModel> AdminCreateInvoiceAsync(CreateManualInvoiceViewModel vm)
            {

                using var res = await _http.PostAsJsonAsync("Invoices/admin-invoice-creation", vm);
                if (!res.IsSuccessStatusCode)
                {
                    var text = await res.Content.ReadAsStringAsync();
                    throw new Exception($"Invoice API failed ({res.StatusCode}): {text}");
                }

                return await res.Content.ReadFromJsonAsync<InvoiceModel>()
                       ?? throw new Exception("Empty response from AdminCreateInvoice");
            }
        }
    }
}
