using BookIt.DAL.Models;
using BookIt.BLL.DTOs;

namespace BookIt.BLL.Services;

public interface IPaymentService
{
    Task<IEnumerable<PaymentDetailsDto>> GetAllPaymentsAsync();
    Task<PaymentDetailsDto?> GetPaymentByIdAsync(int id);
    Task<int> CreatePaymentAsync(CreatePaymentDto dto);
    Task<string?> CreateMonoInvoiceAsync(int paymentId);
    Task<bool> CheckMonoPaymentStatusAsync(ProcessMonoPaymentDto dto);
    Task UpdatePaymentAsync(Payment payment);
    Task DeletePaymentAsync(int id);
}