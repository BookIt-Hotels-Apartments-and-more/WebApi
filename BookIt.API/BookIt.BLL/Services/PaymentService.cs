using BookIt.DAL.Models;
using BookIt.DAL.Repositories;
using BookIt.BLL.Models;
using BookIt.BLL.DTOs;

namespace BookIt.BLL.Services;

public class PaymentService : IPaymentService
{
    private readonly PaymentRepository _paymentRepository;
    private readonly IMonobankAcquiringService _monobankAcquiringService;

    public PaymentService(
        PaymentRepository paymentRepository,
        IMonobankAcquiringService monobankAcquiringService)
    {
        _paymentRepository = paymentRepository;
        _monobankAcquiringService = monobankAcquiringService;
    }

    public async Task<IEnumerable<PaymentDetailsDto>> GetAllPaymentsAsync()
    {
        var payments = await _paymentRepository.GetAllAsync();

        return payments.Select(p => new PaymentDetailsDto
        {
            Id = p.Id,
            Type = p.Type,
            Status = p.Status,
            Amount = p.Amount,
            PaidAt = p.PaidAt,
            BookingId = p.BookingId
        });
    }

    public async Task<PaymentDetailsDto?> GetPaymentByIdAsync(int id)
    {
        var payment = await _paymentRepository.GetByIdAsync(id);
        if (payment == null) return null;

        return new PaymentDetailsDto
        {
            Id = payment.Id,
            Type = payment.Type,
            Status = payment.Status,
            Amount = payment.Amount,
            PaidAt = payment.PaidAt,
            BookingId = payment.BookingId
        };
    }

    public async Task<int> CreatePaymentAsync(CreatePaymentDto dto)
    {
        var payment = new Payment
        {
            Type = dto.Type,
            Amount = dto.Amount,
            BookingId = dto.BookingId,
            Status = PaymentStatus.Pending
        };

        await _paymentRepository.AddAsync(payment);
        return payment.Id;
    }

    public async Task<string?> CreateMonoInvoiceAsync(int paymentId)
    {
        var payment = await _paymentRepository.GetByIdAsync(paymentId);
        if (payment == null || payment.Type != PaymentType.Mono)
            return null;

        var invoiceRequest = new CreateInvoiceRequest
        {
            Amount = ((int)(payment.Amount * 100)).ToString(), // копійки як рядок
            MerchantPaymInfo = $"Оплата бронювання #{payment.Id}",
            RedirectUrl = "https://yourapp.com/payment/success", // 🔁 підставити свій URL
            WebHookUrl = "https://yourapp.com/api/monobank/webhook" // 🔁 підставити свій webhook
        };

        var response = await _monobankAcquiringService.CreateInvoiceAsync(invoiceRequest);
        return response?.PageUrl;
    }

    public async Task<bool> CheckMonoPaymentStatusAsync(ProcessMonoPaymentDto dto)
    {
        var payment = await _paymentRepository.GetByIdAsync(dto.PaymentId);
        if (payment == null || payment.Type != PaymentType.Mono) return false;

        var statusResponse = await _monobankAcquiringService.GetInvoiceStatusAsync(dto.InvoiceId);
        if (statusResponse == null) return false;

        if (statusResponse.Status == "success")
        {
            payment.Status = PaymentStatus.Completed;
            payment.PaidAt = DateTime.UtcNow;
            await _paymentRepository.UpdateAsync(payment);
            return true;
        }

        if (statusResponse.Status == "failure")
        {
            payment.Status = PaymentStatus.Failed;
            await _paymentRepository.UpdateAsync(payment);
        }

        return false;
    }

    public async Task UpdatePaymentAsync(Payment payment)
    {
        await _paymentRepository.UpdateAsync(payment);
    }

    public async Task DeletePaymentAsync(int id)
    {
        await _paymentRepository.DeleteAsync(id);
    }
}
