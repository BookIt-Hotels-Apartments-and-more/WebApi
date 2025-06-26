using Microsoft.AspNetCore.Mvc;
using BookIt.BLL.Services;
using BookIt.BLL.DTOs;
using BookIt.DAL.Models;
using BookIt.BLL.Models;

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

    /// <summary>Перевірити статус інвойсу у Monobank</summary>
    [HttpPost("mono-status")]
    public async Task<IActionResult> CheckMonoStatus([FromBody] ProcessMonoPaymentDto dto)
    {
        var result = await _paymentService.CheckMonoPaymentStatusAsync(dto);
        return result ? Ok("Оплата підтверджена.") : BadRequest("Оплата не підтверджена.");
    }

    /// <summary>Універсальне створення платежу (Mono, Cash, BankTransfer)</summary>
    [HttpPost("universal")]
    public async Task<IActionResult> CreateUniversal([FromBody] CreateUniversalPayment dto)
    {
        Console.WriteLine("est contac2t");
        var result = await _paymentService.CreateUniversalPaymentAsync(dto);
        if (result == null) return BadRequest("Не вдалося створити платіж.");

        return Ok(result);
    }


    /// <summary>Підтвердити платіж вручну (Cash або BankTransfer)</summary>
    [HttpPost("manual-confirm")]
    public async Task<IActionResult> ConfirmManual([FromBody] ManualConfirmPaymentDto dto)
    {
        var success = await _paymentService.ConfirmManualPaymentAsync(dto.PaymentId);
        return success ? Ok("Платіж оновлено.") : BadRequest("Не вдалося оновити платіж.");
    }
}
