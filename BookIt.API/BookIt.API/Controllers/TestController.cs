using Microsoft.AspNetCore.Mvc;
using BookIt.BLL.Interfaces;

namespace BookIt.API.Controllers;
[ApiController]
[Route("test")]
public class TestController : ControllerBase
{
    private readonly ITestService _testService;

    public TestController(ITestService testService)
    {
        _testService = testService;
    }

    [HttpGet("check-connection")]
    public async Task<IActionResult> CheckConnection()
    {
        bool canConnect = await _testService.CanConnectToDatabase();

        if (canConnect)
            return Ok("Подключение к базе успешно");
        else
            return StatusCode(500, "Не удалось подключиться к базе");
    }
}
