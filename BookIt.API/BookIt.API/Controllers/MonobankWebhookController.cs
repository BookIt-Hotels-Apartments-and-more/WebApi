using BookIt.API.Models.Requests;
using BookIt.BLL.Services;
using BookIt.DAL.Configuration.Settings;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace BookIt.API.Controllers;

[ApiController]
[Route("api/monobank/webhook/{secret}")]
public class MonobankWebhookController : ControllerBase
{
    private readonly IPaymentService _service;
    private readonly IOptions<MonobankSettings> _monobankSettingsOptions;

    public MonobankWebhookController(IPaymentService service, IOptions<MonobankSettings> monobankSettingsOptions)
    {
        _service = service;
        _monobankSettingsOptions = monobankSettingsOptions;
    }

    [HttpPost]
    public async Task<IActionResult> ReceiveWebhook([FromRoute] string secret, [FromBody] MonobankWebhookRequest payload)
    {
        var expectedSecret = _monobankSettingsOptions.Value.WebhookSecret;
        if (secret != expectedSecret) return Unauthorized("Invalid webhook secret");
        if (payload.Status != "success") return Ok();

        var bookingIdStr = payload.Reference.Replace("BOOKING-", "");
        if (!int.TryParse(bookingIdStr, out int bookingId)) return BadRequest("Invalid bookingId");
        return await _service.MarkPaymentAsCompletedAsync(bookingId) ? Ok() : NotFound();
    }
}
