using BookIt.BLL.Exceptions;
using BookIt.BLL.Models;
using BookIt.DAL.Configuration.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace BookIt.BLL.Services;

public class MonobankAcquiringService : IMonobankAcquiringService
{
    private readonly string _webHookUrl;
    private readonly HttpClient _httpClient;
    private readonly ILogger<MonobankAcquiringService> _logger;

    public MonobankAcquiringService(
        HttpClient httpClient,
        IOptions<MonobankSettings> options,
        ILogger<MonobankAcquiringService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _webHookUrl = $"{options.Value.WebhookBaseUrl.TrimEnd('/')}/api/monobank/webhook/{options.Value.WebhookSecret}";
    }

    public async Task<CreateInvoiceResponse?> CreateInvoiceAsync(CreateInvoiceRequest request)
    {
        try
        {
            ValidateCreateInvoiceRequest(request);

            request.WebHookUrl = _webHookUrl;

            _logger.LogInformation("Creating Monobank invoice for amount {Amount} UAH", request.Amount);

            var response = await _httpClient.PostAsJsonAsync("/api/merchant/invoice/create", request);

            if (!response.IsSuccessStatusCode)
                await HandleMonobankErrorResponseAsync(response, "CreateInvoice");

            var invoiceResponse = await DeserializeResponseAsync<CreateInvoiceResponse>(response);

            _logger.LogInformation("Successfully created Monobank invoice with ID: {InvoiceId}",
                invoiceResponse?.InvoiceId);
            _logger.LogInformation("Successfully processed Monobank invoice with URL: {_webHookUrl}",
                _webHookUrl);


            return invoiceResponse;
        }
        catch (BookItBaseException)
        {
            throw;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Network error while creating Monobank invoice");
            throw new ExternalServiceException("Monobank", "Network error during invoice creation", ex);
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex, "Timeout while creating Monobank invoice");
            throw new ExternalServiceException("Monobank", "Request timeout during invoice creation", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while creating Monobank invoice");
            throw new ExternalServiceException("Monobank", "Failed to create invoice", ex);
        }
    }

    public async Task<InvoiceStatusResponse?> GetInvoiceStatusAsync(string invoiceId)
    {
        try
        {
            ValidateInvoiceId(invoiceId);

            _logger.LogInformation("Getting Monobank invoice status for ID: {InvoiceId}", invoiceId);

            var response = await _httpClient.GetAsync($"/api/merchant/invoice/status?invoiceId={Uri.EscapeDataString(invoiceId)}");

            if (!response.IsSuccessStatusCode)
                await HandleMonobankErrorResponseAsync(response, "GetInvoiceStatus");

            var statusResponse = await DeserializeResponseAsync<InvoiceStatusResponse>(response);

            _logger.LogInformation("Successfully retrieved Monobank invoice status for ID: {InvoiceId}, Status: {Status}",
                invoiceId, statusResponse?.Status);

            return statusResponse;
        }
        catch (BookItBaseException)
        {
            throw;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Network error while getting Monobank invoice status for ID: {InvoiceId}", invoiceId);
            throw new ExternalServiceException("Monobank", "Network error during invoice status check", ex);
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex, "Timeout while getting Monobank invoice status for ID: {InvoiceId}", invoiceId);
            throw new ExternalServiceException("Monobank", "Request timeout during invoice status check", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while getting Monobank invoice status for ID: {InvoiceId}", invoiceId);
            throw new ExternalServiceException("Monobank", "Failed to get invoice status", ex);
        }
    }

    private void ValidateCreateInvoiceRequest(CreateInvoiceRequest request)
    {
        var validationErrors = new Dictionary<string, List<string>>();

        if (request == null)
            validationErrors.Add("Request", new List<string> { "Create invoice request cannot be null" });

        if (request?.Amount <= 0)
            validationErrors.Add("Amount", new List<string> { "Invoice amount must be greater than 0" });

        if (request?.Amount > 1_000_000)
            validationErrors.Add("Amount", new List<string> { "Invoice amount cannot exceed 1,000,000 kopecks" });

        if (string.IsNullOrWhiteSpace(request?.MerchantPaymInfo?.Reference))
            validationErrors.Add("Reference", new List<string> { "Payment reference is required" });

        if (string.IsNullOrWhiteSpace(request?.MerchantPaymInfo?.Destination))
            validationErrors.Add("Destination", new List<string> { "Payment destination is required" });

        if (validationErrors.Any())
            throw new ValidationException(validationErrors);
    }

    private void ValidateInvoiceId(string invoiceId)
    {
        if (string.IsNullOrWhiteSpace(invoiceId))
            throw new ValidationException("InvoiceId", "Invoice ID is required");
    }

    private async Task HandleMonobankErrorResponseAsync(HttpResponseMessage response, string operation)
    {
        try
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogError("Monobank {Operation} failed with status {StatusCode}: {Error}",
                operation, response.StatusCode, errorContent);

            Exception exception = response.StatusCode switch
            {
                HttpStatusCode.BadRequest => new ValidationException("Request", $"Invalid request to Monobank: {errorContent}"),
                HttpStatusCode.Unauthorized => new ExternalServiceException("Monobank", "Invalid API token or unauthorized access", null!, 401),
                HttpStatusCode.Forbidden => new ExternalServiceException("Monobank", "Access forbidden - check API permissions", null!, 403),
                HttpStatusCode.NotFound => new EntityNotFoundException("Invoice", "requested invoice"),
                HttpStatusCode.TooManyRequests => new ExternalServiceException("Monobank", "Rate limit exceeded", null!, 429),
                HttpStatusCode.InternalServerError => new ExternalServiceException("Monobank", "Monobank internal server error", null!, 500),
                HttpStatusCode.BadGateway => new ExternalServiceException("Monobank", "Monobank service unavailable", null!, 502),
                HttpStatusCode.ServiceUnavailable => new ExternalServiceException("Monobank", "Monobank service temporarily unavailable", null!, 503),
                _ => new ExternalServiceException("Monobank", $"Monobank {operation} failed: {response.StatusCode} - {errorContent}", null!, (int)response.StatusCode)
            };

            throw exception;
        }
        catch (BookItBaseException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling Monobank error response");
            throw new ExternalServiceException("Monobank", "Failed to process Monobank error response", ex);
        }
    }

    private async Task<T?> DeserializeResponseAsync<T>(HttpResponseMessage response) where T : class
    {
        try
        {
            var content = await response.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(content))
                throw new ExternalServiceException("Monobank", "Received empty response from Monobank");

            var result = JsonSerializer.Deserialize<T>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (result is null)
                throw new ExternalServiceException("Monobank", "Failed to deserialize Monobank response");

            return result;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to deserialize Monobank response");
            throw new ExternalServiceException("Monobank", "Invalid JSON response from Monobank", ex);
        }
        catch (BookItBaseException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deserializing Monobank response");
            throw new ExternalServiceException("Monobank", "Failed to process Monobank response", ex);
        }
    }
}