using BookIt.BLL.Models;
using BookIt.DAL.Configuration.Settings;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace BookIt.BLL.Services;

public class MonobankAcquiringService : IMonobankAcquiringService
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;
    private readonly string _token;
    private readonly string _webhookSecret;

    public MonobankAcquiringService(HttpClient httpClient, IOptions<MonobankSettings> options)
    {
        var settings = options.Value;
        _baseUrl = settings.BaseUrl.TrimEnd('/');
        _token = settings.Token;
        _webhookSecret = settings.WebhookSecret;

        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri(_baseUrl);
        _httpClient.DefaultRequestHeaders.Add("X-Token", _token);
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    }

    public async Task<CreateInvoiceResponse?> CreateInvoiceAsync(CreateInvoiceRequest request)
    {
        request.WebHookUrl = $"https://bbba-46-98-138-86.ngrok-free.app/api/monobank/webhook/{_webhookSecret}";
        var response = await _httpClient.PostAsJsonAsync("/api/merchant/invoice/create", request);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException($"Monobank CreateInvoice failed: {response.StatusCode}, {error}");
        }

        return await response.Content.ReadFromJsonAsync<CreateInvoiceResponse>();
    }

    public async Task<InvoiceStatusResponse?> GetInvoiceStatusAsync(string invoiceId)
    {
        var response = await _httpClient.GetAsync($"/api/merchant/invoice/status?invoiceId={invoiceId}");

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException($"Monobank GetInvoiceStatus failed: {response.StatusCode}, {error}");
        }

        return await response.Content.ReadFromJsonAsync<InvoiceStatusResponse>();
    }
}
