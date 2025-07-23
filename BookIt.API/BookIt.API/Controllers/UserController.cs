using Microsoft.AspNetCore.Mvc;
using BookIt.BLL.Services;

namespace BookIt.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;

    public UserController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
    
        var users = await _userService.GetUsersAsync();
        return Ok(users);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
        var user = await _userService.GetUserByIdAsync(id);
        if (user == null) return NotFound();
        return Ok(user);
    }

    // [HttpDelete("{id}")]
    // public async Task<IActionResult> Delete(int id)
    // {
    //     await _userService.DeleteUserAsync(id);
    //     return NoContent();
    // }
}
