using BookIt.BLL.DTOs;
using BookIt.BLL.Models;
using BookIt.BLL.Services;
using Microsoft.AspNetCore.Mvc;

namespace BookIt.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaymentController : ControllerBase
{
    private readonly IPaymentService _service;

    public PaymentController(IPaymentService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var payments = await _service.GetAllPaymentsAsync();
        return Ok(payments);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id)
    {
        var payment = await _service.GetPaymentByIdAsync(id);
        return Ok(payment);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePaymentDto dto)
    {
        var paymentId = await _service.CreatePaymentAsync(dto);
        return Ok(new { Id = paymentId });
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete([FromRoute] int id)
    {
        await _service.DeletePaymentAsync(id);
        return NoContent();
    }

    [HttpPost("mono-status")]
    public async Task<IActionResult> CheckMonoStatus([FromBody] ProcessMonoPaymentDto dto)
    {
        var isApproved = await _service.CheckMonoPaymentStatusAsync(dto);
        return isApproved ? Ok(new { Message = "Payment was approved." }) : BadRequest(new { Message = "Payment was not approved." });
    }

    [HttpPost("universal")]
    public async Task<IActionResult> CreateUniversal([FromBody] CreateUniversalPayment dto)
    {
        var result = await _service.CreateUniversalPaymentAsync(dto);
        return result is not null ? Ok(result) : BadRequest(new { Message = "Could not create a payment" });
    }

    [HttpPost("manual-confirm")]
    public async Task<IActionResult> ConfirmManual([FromBody] ManualConfirmPaymentDto dto)
    {
        var isConfirmed = await _service.ConfirmManualPaymentAsync(dto.PaymentId);
        return isConfirmed ? Ok(new { Message = "Payment was confirmed." }) : BadRequest(new { Message = "Could not confirm payment." });
    }
}
