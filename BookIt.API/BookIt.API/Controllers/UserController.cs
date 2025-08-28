using AutoMapper;
using BookIt.API.Models.Responses;
using BookIt.BLL.Interfaces;
using BookIt.DAL.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookIt.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
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

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById([FromRoute] int id)
    {
        var userDto = await _userService.GetUserByIdAsync(id);
        var usersResponse = _mapper.Map<UserResponse>(userDto);
        return Ok(usersResponse);
    }

    [HttpPatch("{id:int}/role/{role:int}")]
    public async Task<IActionResult> ChangeRole([FromRoute] int id, [FromRoute] UserRole role)
    {
        await _userService.ChangeUserRoleAsync(id, role);
        return Ok();
    }

    [HttpPatch("{id:int}/restrict")]
    public async Task<IActionResult> RestrictUser([FromRoute] int id)
    {
        await _userService.RestrictUserAsync(id, true);
        return Ok();
    }

    [HttpPatch("{id:int}/unrestrict")]
    public async Task<IActionResult> UnrestrictUser([FromRoute] int id)
    {
        await _userService.RestrictUserAsync(id, false);
        return Ok();
    }
}
