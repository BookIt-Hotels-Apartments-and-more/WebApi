using BookIt.BLL.Models;

namespace BookIt.BLL.Services;

public interface IMonobankAcquiringService
{
    Task<CreateInvoiceResponse?> CreateInvoiceAsync(CreateInvoiceRequest request);
    Task<InvoiceStatusResponse?> GetInvoiceStatusAsync(string invoiceId);
}