using Microsoft.AspNetCore.Mvc;
using BookIt.BLL.Services;
using BookIt.API.Models.Requests;

namespace BookIt.API.Controllers;

[ApiController]
[Route("api/monobank/webhook/{secret}")]
public class MonobankWebhookController : ControllerBase
{
    private readonly IPaymentService _paymentService;
    private readonly IConfiguration _configuration;

    public MonobankWebhookController(IPaymentService paymentService, IConfiguration configuration)
    {
        _paymentService = paymentService;
        _configuration = configuration;
    }

    [HttpPost]
    public async Task<IActionResult> ReceiveWebhook(
        [FromRoute] string secret,
        [FromBody] MonobankWebhookRequest payload)
    {
        var expectedSecret = _configuration["Monobank:WebhookSecret"];
        if (secret != expectedSecret)
            return Unauthorized("Invalid webhook secret");

        // if (!payload.Reference.StartsWith("BOOKING-"))
        //     return BadRequest("Invalid reference");

        if (payload.Status != "success")
            return Ok();

        var bookingIdStr = payload.Reference.Replace("BOOKING-", "");
        if (!int.TryParse(bookingIdStr, out int bookingId))
            return BadRequest("Invalid bookingId");

        var success = await _paymentService.MarkPaymentAsCompletedAsync(bookingId);
        
        return success ? Ok() : NotFound();
    }
}
