using Microsoft.AspNetCore.Mvc;
using BookIt.BLL.Services;
using BookIt.API.Models.Requests;
using Microsoft.Extensions.Options;
using BookIt.DAL.Configuration.Settings;

namespace BookIt.API.Controllers;

[ApiController]
[Route("api/monobank/webhook/{secret}")]
public class MonobankWebhookController : ControllerBase
{
    private readonly IPaymentService _paymentService;
    private readonly IOptions<MonobankSettings> _monobankSettingsOptions;

    public MonobankWebhookController(
        IPaymentService paymentService,
        IOptions<MonobankSettings> monobankSettingsOptions)
    {
        _paymentService = paymentService;
        _monobankSettingsOptions = monobankSettingsOptions;
    }

    [HttpPost]
    public async Task<IActionResult> ReceiveWebhook(
        [FromRoute] string secret,
        [FromBody] MonobankWebhookRequest payload)
    {
        var expectedSecret = _monobankSettingsOptions.Value.WebhookSecret;
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
