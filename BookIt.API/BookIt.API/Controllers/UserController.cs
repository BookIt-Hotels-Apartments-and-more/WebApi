using Microsoft.AspNetCore.Mvc;
using BookIt.BLL.Services;
using AutoMapper;
using BookIt.API.Models.Responses;

namespace BookIt.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly IMapper _mapper;
    private readonly IUserService _userService;

    public UserController(IMapper mapper, IUserService userService)
    {
        _mapper = mapper;
        _userService = userService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var usersDto = await _userService.GetUsersAsync();
        var usersResponse = _mapper.Map<IEnumerable<UserResponse>>(usersDto);
        return Ok(usersResponse);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var userDto = await _userService.GetUserByIdAsync(id);
        if (userDto is null) return NotFound();
        var usersResponse = _mapper.Map<UserResponse>(userDto);
        return Ok(usersResponse);
    }

    // [HttpDelete("{id}")]
    // public async Task<IActionResult> Delete(int id)
    // {
    //     await _userService.DeleteUserAsync(id);
    //     return NoContent();
    // }
}
