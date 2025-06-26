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
            InvoiceUrl = payment.InvoiceUrl,
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
            Amount = (int)(payment.Amount * 100),
            Ccy = 980,
            MerchantPaymInfo = new MerchantPaymInfo
            {
                Reference = $"BOOKING-{payment.Id}",
                Destination = $"Оплата бронювання #{payment.Id}"
            },
            RedirectUrl = "https://yourapp.com/payment/success",
            WebHookUrl = "https://yourapp.com/api/monobank/webhook"
        };

        var response = await _monobankAcquiringService.CreateInvoiceAsync(invoiceRequest);
        return response?.PageUrl;
    }

    public async Task<bool> ConfirmManualPaymentAsync(int paymentId)
    {
        var payment = await _paymentRepository.GetByIdAsync(paymentId);
        if (payment == null)
            return false;

        // Перевіряємо, чи це ручний тип платежу
        if (payment.Type != PaymentType.Cash && payment.Type != PaymentType.BankTransfer)
            return false;

        payment.Status = PaymentStatus.Completed;
        payment.PaidAt = DateTime.UtcNow;

        await _paymentRepository.UpdateAsync(payment);
        return true;
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

    public async Task<UniversalPaymentResponse?> CreateUniversalPaymentAsync(CreateUniversalPayment dto)
    {
        PaymentDetailsDto? paymentExist = await this.GetPaymentByIdAsync(dto.BookingId);

        if (paymentExist != null)
        {
            return new UniversalPaymentResponse
            {
                PaymentId = paymentExist.Id,
                Type = paymentExist.Type,
                PaidAt = paymentExist.PaidAt,
                InvoiceUrl = paymentExist.InvoiceUrl
            };
        }

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
            var invoiceRequest = new CreateInvoiceRequest
            {
                Amount = (int)(payment.Amount * 100),
                Ccy = 980,
                MerchantPaymInfo = new MerchantPaymInfo
                {
                    Reference = $"BOOKING-{dto.BookingId}",
                    Destination = $"Оплата бронювання #{payment.Id}"
                },
                RedirectUrl = "https://yourapp.com/payment/success",
            };

            var invoice = await _monobankAcquiringService.CreateInvoiceAsync(invoiceRequest);
            invoiceUrl = invoice?.PageUrl;

            payment.InvoiceUrl = invoiceUrl;

            await _paymentRepository.UpdateAsync(payment);
        }

        return new UniversalPaymentResponse
        {
            PaymentId = payment.Id,
            Type = payment.Type,
            PaidAt = payment.PaidAt,
            InvoiceUrl = invoiceUrl,
        };
    }

    public async Task<bool> MarkPaymentAsCompletedAsync(int bookingId)
    {
        var payment = await _paymentRepository.GetByBookingIdAsync(bookingId);
        if (payment == null) return false;

        payment.Status = PaymentStatus.Completed;
        payment.PaidAt = DateTime.UtcNow;
        await _paymentRepository.UpdateAsync(payment);

        return true;
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
