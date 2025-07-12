using Microsoft.AspNetCore.Mvc;
using BookIt.BLL.Services;
using BookIt.BLL.DTOs;
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

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var payments = await _paymentService.GetAllPaymentsAsync();
        return Ok(payments);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
        var payment = await _paymentService.GetPaymentByIdAsync(id);
        if (payment == null) return NotFound();
        return Ok(payment);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePaymentDto dto)
    {
        var paymentId = await _paymentService.CreatePaymentAsync(dto);
        return CreatedAtAction(nameof(Get), new { id = paymentId }, new { id = paymentId });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _paymentService.DeletePaymentAsync(id);
        return NoContent();
    }

    [HttpPost("mono-status")]
    public async Task<IActionResult> CheckMonoStatus([FromBody] ProcessMonoPaymentDto dto)
    {
        var result = await _paymentService.CheckMonoPaymentStatusAsync(dto);
        return result 
            ? Ok("Payment was approved.")
            : BadRequest("Payment was not approved.");
    }

    [HttpPost("universal")]
    public async Task<IActionResult> CreateUniversal([FromBody] CreateUniversalPayment dto)
    {
        var result = await _paymentService.CreateUniversalPaymentAsync(dto);
        return result is not null
            ? Ok(result)
            : BadRequest("Could not create a payment");
    }

    [HttpPost("manual-confirm")]
    public async Task<IActionResult> ConfirmManual([FromBody] ManualConfirmPaymentDto dto)
    {
        var success = await _paymentService.ConfirmManualPaymentAsync(dto.PaymentId);
        return success ? Ok("Payment was confirmed.") : BadRequest("Could not confirm payment.");
    }
}
