using Microsoft.AspNetCore.Mvc;
using BookIt.Database;

namespace BookIt.API.Controllers;
[ApiController]
[Route("test")]
public class TestController : ControllerBase
{
    private readonly DatabaseContext _context;

    public TestController(DatabaseContext context)
    {
        _context = context;
    }

    [HttpGet("check-connection")]
    public async Task<IActionResult> CheckConnection()
    {
        bool canConnect = await _context.Database.CanConnectAsync();

        if (canConnect)
            return Ok("Подключение к базе успешно");
        else
            return StatusCode(500, "Не удалось подключиться к базе");
    }
}
