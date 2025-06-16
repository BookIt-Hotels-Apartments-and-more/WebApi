using Microsoft.AspNetCore.Mvc;
using BookIt.BLL.Services;
using BookIt.BLL.DTOs;
using BookIt.DAL.Models;

namespace BookIt.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaymentController : ControllerBase
{
    private readonly IPaymentService _paymentService;

    public PaymentController(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    /// <summary>Отримати всі платежі</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var payments = await _paymentService.GetAllPaymentsAsync();
        return Ok(payments);
    }

    /// <summary>Отримати конкретний платіж</summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
        var payment = await _paymentService.GetPaymentByIdAsync(id);
        if (payment == null) return NotFound();
        return Ok(payment);
    }

    /// <summary>Створити платіж</summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePaymentDto dto)
    {
        var paymentId = await _paymentService.CreatePaymentAsync(dto);
        return CreatedAtAction(nameof(Get), new { id = paymentId }, new { id = paymentId });
    }

    /// <summary>Видалити платіж</summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _paymentService.DeletePaymentAsync(id);
        return NoContent();
    }

    /// <summary>Створити інвойс у Monobank для оплати</summary>
    [HttpPost("{paymentId}/mono-invoice")]
    public async Task<IActionResult> CreateMonoInvoice(int paymentId)
    {
        var url = await _paymentService.CreateMonoInvoiceAsync(paymentId);
        if (url == null) return BadRequest("Не вдалося створити інвойс або тип платежу не Monobank.");
        return Ok(new { invoiceUrl = url });
    }

    /// <summary>Перевірити статус інвойсу у Monobank</summary>
    [HttpPost("mono-status")]
    public async Task<IActionResult> CheckMonoStatus([FromBody] ProcessMonoPaymentDto dto)
    {
        var result = await _paymentService.CheckMonoPaymentStatusAsync(dto);
        return result ? Ok("Оплата підтверджена.") : BadRequest("Оплата не підтверджена.");
    }

    // /// <summary>Підтвердити платіж вручну (Cash або BankTransfer)</summary>
    // [HttpPost("manual-confirm")]
    // public async Task<IActionResult> ConfirmManual([FromBody] ManualConfirmPaymentDto dto)
    // {
    //     var success = await _paymentService.ConfirmManualPaymentAsync(dto);
    //     return success ? Ok("Платіж оновлено.") : BadRequest("Не вдалося оновити платіж.");
    // }
}
