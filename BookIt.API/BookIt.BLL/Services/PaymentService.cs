using BookIt.BLL.DTOs;
using BookIt.BLL.Exceptions;
using BookIt.BLL.Models;
using BookIt.DAL.Enums;
using BookIt.DAL.Models;
using BookIt.DAL.Repositories;
using Microsoft.Extensions.Logging;

namespace BookIt.BLL.Services;

public class PaymentService : IPaymentService
{
    private readonly PaymentRepository _paymentRepository;
    private readonly BookingsRepository _bookingsRepository;
    private readonly IMonobankAcquiringService _monobankAcquiringService;
    private readonly ILogger<PaymentService> _logger;

    public PaymentService(
        PaymentRepository paymentRepository,
        BookingsRepository bookingsRepository,
        IMonobankAcquiringService monobankAcquiringService,
        ILogger<PaymentService> logger)
    {
        _paymentRepository = paymentRepository ?? throw new ArgumentNullException(nameof(paymentRepository));
        _bookingsRepository = bookingsRepository ?? throw new ArgumentNullException(nameof(bookingsRepository));
        _monobankAcquiringService = monobankAcquiringService ?? throw new ArgumentNullException(nameof(monobankAcquiringService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<IEnumerable<PaymentDetailsDto>> GetAllPaymentsAsync()
    {
        try
        {
            var payments = await _paymentRepository.GetAllAsync();

            return payments.Select(p => new PaymentDetailsDto
            {
                Id = p.Id,
                Type = p.Type,
                Status = p.Status,
                Amount = p.Amount,
                InvoiceUrl = p.InvoiceUrl,
                PaidAt = p.PaidAt,
                BookingId = p.BookingId
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve all payments");
            throw new ExternalServiceException("Database", "Failed to retrieve payments", ex);
        }
    }

    public async Task<PaymentDetailsDto?> GetPaymentByIdAsync(int id)
    {
        try
        {
            var payment = await _paymentRepository.GetByIdAsync(id);
            if (payment is null)
                throw new EntityNotFoundException("Payment", id);

            return new PaymentDetailsDto
            {
                Id = payment.Id,
                Type = payment.Type,
                Status = payment.Status,
                Amount = payment.Amount,
                InvoiceUrl = payment.InvoiceUrl,
                PaidAt = payment.PaidAt,
                BookingId = payment.BookingId
            };
        }
        catch (BookItBaseException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve payment by ID: {PaymentId}", id);
            throw new ExternalServiceException("Database", "Failed to retrieve payment", ex);
        }
    }

    public async Task<int> CreatePaymentAsync(CreatePaymentDto dto)
    {
        try
        {
            ValidateCreatePaymentData(dto);

            await ValidateBookingExistsAsync(dto.BookingId);

            var existingPayment = await _paymentRepository.GetByBookingIdAsync(dto.BookingId);
            if (existingPayment is not null)
                throw new EntityAlreadyExistsException("Payment", "booking", dto.BookingId.ToString());

            _logger.LogInformation("Creating payment for booking {BookingId}, amount: {Amount}", dto.BookingId, dto.Amount);

            var payment = new Payment
            {
                Type = dto.Type,
                Amount = dto.Amount,
                BookingId = dto.BookingId,
                Status = PaymentStatus.Pending
            };

            await _paymentRepository.AddAsync(payment);

            _logger.LogInformation("Successfully created payment with ID: {PaymentId}", payment.Id);

            return payment.Id;
        }
        catch (BookItBaseException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create payment for booking {BookingId}", dto?.BookingId);
            throw new ExternalServiceException("Database", "Failed to create payment", ex);
        }
    }

    public async Task<string?> CreateMonoInvoiceAsync(int paymentId)
    {
        try
        {
            var payment = await _paymentRepository.GetByIdAsync(paymentId);
            if (payment is null)
                throw new EntityNotFoundException("Payment", paymentId);

            if (payment.Type != PaymentType.Mono)
                throw new BusinessRuleViolationException("INVALID_PAYMENT_TYPE",
                    $"Cannot create Monobank invoice for payment type: {payment.Type}");

            _logger.LogInformation("Creating Monobank invoice for payment {PaymentId}", paymentId);

            var invoiceRequest = new CreateInvoiceRequest
            {
                Amount = ConvertToKopecks(payment.Amount),
                Ccy = 980,
                MerchantPaymInfo = new MerchantPaymInfo
                {
                    Reference = $"BOOKING-{payment.BookingId}",
                    Destination = $"Оплата бронювання #{payment.BookingId}"
                }
            };

            var response = await _monobankAcquiringService.CreateInvoiceAsync(invoiceRequest);
            if (response?.PageUrl is null)
                throw new ExternalServiceException("Monobank", "Failed to create invoice - no page URL returned");

            payment.InvoiceUrl = response.PageUrl;
            await _paymentRepository.UpdateAsync(payment);

            _logger.LogInformation("Successfully created Monobank invoice for payment {PaymentId}", paymentId);

            return response.PageUrl;
        }
        catch (BookItBaseException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create Monobank invoice for payment {PaymentId}", paymentId);
            throw new ExternalServiceException("Payment", "Failed to create Monobank invoice", ex);
        }
    }

    public async Task<bool> ConfirmManualPaymentAsync(int paymentId)
    {
        try
        {
            var payment = await _paymentRepository.GetByIdAsync(paymentId);
            if (payment is null)
                throw new EntityNotFoundException("Payment", paymentId);

            if (payment.Type != PaymentType.Cash && payment.Type != PaymentType.BankTransfer)
                throw new BusinessRuleViolationException("INVALID_PAYMENT_TYPE",
                    $"Cannot manually confirm payment of type: {payment.Type}");

            if (payment.Status == PaymentStatus.Completed)
                throw new BusinessRuleViolationException("PAYMENT_ALREADY_COMPLETED",
                    "Payment is already completed");

            if (payment.Status == PaymentStatus.Failed)
                throw new BusinessRuleViolationException("PAYMENT_FAILED",
                    "Cannot confirm a failed payment");

            _logger.LogInformation("Confirming manual payment {PaymentId}", paymentId);

            payment.Status = PaymentStatus.Completed;
            payment.PaidAt = DateTime.UtcNow;

            await _paymentRepository.UpdateAsync(payment);

            _logger.LogInformation("Successfully confirmed manual payment {PaymentId}", paymentId);

            return true;
        }
        catch (BookItBaseException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to confirm manual payment {PaymentId}", paymentId);
            throw new ExternalServiceException("Database", "Failed to confirm manual payment", ex);
        }
    }

    public async Task<bool> CheckMonoPaymentStatusAsync(ProcessMonoPaymentDto dto)
    {
        try
        {
            ValidateProcessMonoPaymentData(dto);

            var payment = await _paymentRepository.GetByIdAsync(dto.PaymentId);
            if (payment is null)
                throw new EntityNotFoundException("Payment", dto.PaymentId);

            if (payment.Type != PaymentType.Mono)
                throw new BusinessRuleViolationException("INVALID_PAYMENT_TYPE",
                    $"Payment {dto.PaymentId} is not a Monobank payment");

            _logger.LogInformation("Checking Monobank payment status for payment {PaymentId}, invoice {InvoiceId}",
                dto.PaymentId, dto.InvoiceId);

            var statusResponse = await _monobankAcquiringService.GetInvoiceStatusAsync(dto.InvoiceId);
            if (statusResponse is null)
                throw new ExternalServiceException("Monobank", "Failed to get invoice status from Monobank");

            var wasUpdated = false;

            if (statusResponse.Status == "success")
            {
                if (payment.Status != PaymentStatus.Completed)
                {
                    payment.Status = PaymentStatus.Completed;
                    payment.PaidAt = DateTime.UtcNow;
                    await _paymentRepository.UpdateAsync(payment);
                    wasUpdated = true;

                    _logger.LogInformation("Monobank payment {PaymentId} completed successfully", dto.PaymentId);
                }
            }
            else if (statusResponse.Status == "failure" || statusResponse.Status == "expired")
            {
                if (payment.Status != PaymentStatus.Failed)
                {
                    payment.Status = PaymentStatus.Failed;
                    await _paymentRepository.UpdateAsync(payment);
                    wasUpdated = true;

                    _logger.LogWarning("Monobank payment {PaymentId} failed with status: {Status}",
                        dto.PaymentId, statusResponse.Status);
                }
            }

            return wasUpdated && payment.Status == PaymentStatus.Completed;
        }
        catch (BookItBaseException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to check Monobank payment status for payment {PaymentId}", dto?.PaymentId);
            throw new ExternalServiceException("Payment", "Failed to check Monobank payment status", ex);
        }
    }

    public async Task<UniversalPaymentResponse?> CreateUniversalPaymentAsync(CreateUniversalPayment dto)
    {
        try
        {
            ValidateCreateUniversalPaymentData(dto);

            await ValidateBookingExistsAsync(dto.BookingId);

            var existingPayment = await _paymentRepository.GetByBookingIdAsync(dto.BookingId);
            if (existingPayment is not null)
            {
                _logger.LogInformation("Payment already exists for booking {BookingId}, returning existing payment {PaymentId}",
                    dto.BookingId, existingPayment.Id);

                return new UniversalPaymentResponse
                {
                    PaymentId = existingPayment.Id,
                    Type = existingPayment.Type,
                    PaidAt = existingPayment.PaidAt,
                    InvoiceUrl = existingPayment.InvoiceUrl
                };
            }

            _logger.LogInformation("Creating universal payment for booking {BookingId}, type: {Type}, amount: {Amount}",
                dto.BookingId, dto.Type, dto.Amount);

            var payment = new Payment
            {
                Type = dto.Type,
                Amount = dto.Amount,
                BookingId = dto.BookingId,
                Status = PaymentStatus.Pending,
                PaidAt = DateTime.UtcNow
            };

            await _paymentRepository.AddAsync(payment);

            string? invoiceUrl = null;
            if (dto.Type == PaymentType.Mono)
            {
                try
                {
                    var invoiceRequest = new CreateInvoiceRequest
                    {
                        Amount = ConvertToKopecks(dto.Amount),
                        Ccy = 980,
                        MerchantPaymInfo = new MerchantPaymInfo
                        {
                            Reference = $"BOOKING-{dto.BookingId}",
                            Destination = $"Booking #{dto.BookingId} payment"
                        }
                    };

                    var invoice = await _monobankAcquiringService.CreateInvoiceAsync(invoiceRequest);
                    invoiceUrl = invoice?.PageUrl;

                    if (invoiceUrl is not null)
                    {
                        payment.InvoiceUrl = invoiceUrl;
                        await _paymentRepository.UpdateAsync(payment);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to create Monobank invoice for payment {PaymentId}", payment.Id);
                    payment.Status = PaymentStatus.Failed;
                    await _paymentRepository.UpdateAsync(payment);
                }
            }

            _logger.LogInformation("Successfully created universal payment {PaymentId} for booking {BookingId}",
                payment.Id, dto.BookingId);

            return new UniversalPaymentResponse
            {
                PaymentId = payment.Id,
                Type = payment.Type,
                PaidAt = payment.PaidAt,
                InvoiceUrl = invoiceUrl,
            };
        }
        catch (BookItBaseException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create universal payment for booking {BookingId}", dto?.BookingId);
            throw new ExternalServiceException("Payment", "Failed to create universal payment", ex);
        }
    }

    public async Task<bool> MarkPaymentAsCompletedAsync(int bookingId)
    {
        try
        {
            var payment = await _paymentRepository.GetByBookingIdAsync(bookingId);
            if (payment is null)
            {
                throw new EntityNotFoundException("Payment", $"booking {bookingId}");
            }

            if (payment.Status == PaymentStatus.Completed)
            {
                _logger.LogInformation("Payment {PaymentId} for booking {BookingId} is already completed",
                    payment.Id, bookingId);
                return true;
            }

            if (payment.Status == PaymentStatus.Failed)
            {
                throw new BusinessRuleViolationException("PAYMENT_FAILED",
                    "Cannot complete a failed payment");
            }

            _logger.LogInformation("Marking payment {PaymentId} as completed for booking {BookingId}",
                payment.Id, bookingId);

            payment.Status = PaymentStatus.Completed;
            payment.PaidAt = DateTime.UtcNow;
            await _paymentRepository.UpdateAsync(payment);

            return true;
        }
        catch (BookItBaseException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to mark payment as completed for booking {BookingId}", bookingId);
            throw new ExternalServiceException("Database", "Failed to mark payment as completed", ex);
        }
    }

    public async Task UpdatePaymentAsync(Payment payment)
    {
        try
        {
            if (payment is null) throw new ValidationException("Payment", "Payment cannot be null");
            if (payment.Id <= 0) throw new ValidationException("PaymentId", "Valid payment ID is required");

            var existingPayment = await _paymentRepository.GetByIdAsync(payment.Id);
            if (existingPayment is null)
            {
                throw new EntityNotFoundException("Payment", payment.Id);
            }

            await _paymentRepository.UpdateAsync(payment);

            _logger.LogInformation("Successfully updated payment {PaymentId}", payment.Id);
        }
        catch (BookItBaseException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update payment {PaymentId}", payment?.Id);
            throw new ExternalServiceException("Database", "Failed to update payment", ex);
        }
    }

    public async Task DeletePaymentAsync(int id)
    {
        try
        {
            var payment = await _paymentRepository.GetByIdAsync(id);
            if (payment is null) throw new EntityNotFoundException("Payment", id);

            if (payment.Status == PaymentStatus.Completed)
                throw new BusinessRuleViolationException("PAYMENT_COMPLETED",
                    "Cannot delete a completed payment");

            await _paymentRepository.DeleteAsync(id);

            _logger.LogInformation("Successfully deleted payment {PaymentId}", id);
        }
        catch (BookItBaseException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete payment {PaymentId}", id);
            throw new ExternalServiceException("Database", "Failed to delete payment", ex);
        }
    }

    private void ValidateCreatePaymentData(CreatePaymentDto dto)
    {
        var validationErrors = new Dictionary<string, List<string>>();

        if (dto is null)
            validationErrors.Add("Payment", new List<string> { "Payment data cannot be null" });

        if (dto?.BookingId <= 0)
            validationErrors.Add("BookingId", new List<string> { "Valid booking ID is required" });

        if (dto?.Amount <= 0)
            validationErrors.Add("Amount", new List<string> { "Payment amount must be greater than 0" });

        if (dto?.Amount > 1_000_000)
            validationErrors.Add("Amount", new List<string> { "Payment amount cannot exceed 1,000,000" });

        if (!Enum.IsDefined(typeof(PaymentType), dto?.Type ?? 0))
            validationErrors.Add("Type", new List<string> { "Valid payment type is required" });

        if (validationErrors.Any())
            throw new ValidationException(validationErrors);
    }

    private void ValidateCreateUniversalPaymentData(CreateUniversalPayment dto)
    {
        var validationErrors = new Dictionary<string, List<string>>();

        if (dto is null)
            validationErrors.Add("Payment", new List<string> { "Payment data cannot be null" });

        if (dto?.BookingId <= 0)
            validationErrors.Add("BookingId", new List<string> { "Valid booking ID is required" });

        if (dto?.Amount <= 0)
            validationErrors.Add("Amount", new List<string> { "Payment amount must be greater than 0" });

        if (dto?.Amount > 1_000_000)
            validationErrors.Add("Amount", new List<string> { "Payment amount cannot exceed 1,000,000" });

        if (!Enum.IsDefined(typeof(PaymentType), dto?.Type ?? 0))
            validationErrors.Add("Type", new List<string> { "Valid payment type is required" });

        if (validationErrors.Any())
            throw new ValidationException(validationErrors);
    }

    private void ValidateProcessMonoPaymentData(ProcessMonoPaymentDto dto)
    {
        var validationErrors = new Dictionary<string, List<string>>();

        if (dto is null)
            validationErrors.Add("ProcessData", new List<string> { "Process data cannot be null" });

        if (dto?.PaymentId <= 0)
            validationErrors.Add("PaymentId", new List<string> { "Valid payment ID is required" });

        if (string.IsNullOrWhiteSpace(dto?.InvoiceId))
            validationErrors.Add("InvoiceId", new List<string> { "Invoice ID is required" });

        if (validationErrors.Any())
            throw new ValidationException(validationErrors);
    }

    private async Task ValidateBookingExistsAsync(int bookingId)
    {
        if (!await _bookingsRepository.ExistsAsync(bookingId))
            throw new EntityNotFoundException("Booking", bookingId);
    }

    private int ConvertToKopecks(decimal amount)
    {
        try
        {
            return (int)(amount * 100);
        }
        catch (OverflowException)
        {
            throw new ValidationException("Amount", "Payment amount is too large to convert to kopecks");
        }
    }
}