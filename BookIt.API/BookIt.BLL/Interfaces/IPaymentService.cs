using BookIt.DAL.Models;
using BookIt.BLL.DTOs;
using BookIt.BLL.Models;

namespace BookIt.BLL.Services;

public interface IPaymentService
{
    Task<IEnumerable<PaymentDetailsDto>> GetAllPaymentsAsync();
    Task<PaymentDetailsDto?> GetPaymentByIdAsync(int id);
    Task<int> CreatePaymentAsync(CreatePaymentDto dto);
    Task<string?> CreateMonoInvoiceAsync(int paymentId);
    Task<bool> ConfirmManualPaymentAsync(int paymentId);
    Task<bool> CheckMonoPaymentStatusAsync(ProcessMonoPaymentDto dto);
    Task<UniversalPaymentResponse?> CreateUniversalPaymentAsync(CreateUniversalPayment dto);
    Task<bool> MarkPaymentAsCompletedAsync(int bookingId);
    Task UpdatePaymentAsync(Payment payment);
    Task DeletePaymentAsync(int id);
}