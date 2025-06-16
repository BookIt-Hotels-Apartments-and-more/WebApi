using BookIt.BLL.Models;
using BookIt.BLL.Configuration;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers; // ⬅️ це треба
using System.Net.Http.Json;

namespace BookIt.BLL.Services;

public class MonobankAcquiringService : IMonobankAcquiringService
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;
    private readonly string _token;

    public MonobankAcquiringService(IOptions<MonobankSettings> options, IHttpClientFactory httpClientFactory)
    {
        _baseUrl = options.Value.BaseUrl;
        _token = options.Value.Token;
        _httpClient = httpClientFactory.CreateClient();
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);
    }

    public async Task<CreateInvoiceResponse?> CreateInvoiceAsync(CreateInvoiceRequest request)
    {
        var response = await _httpClient.PostAsJsonAsync($"{_baseUrl}/invoice/create", request);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<CreateInvoiceResponse>();
    }

    public async Task<InvoiceStatusResponse?> GetInvoiceStatusAsync(string invoiceId)
    {
        var response = await _httpClient.GetAsync($"{_baseUrl}/invoice/status?invoiceId={invoiceId}");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<InvoiceStatusResponse>();
    }
}
