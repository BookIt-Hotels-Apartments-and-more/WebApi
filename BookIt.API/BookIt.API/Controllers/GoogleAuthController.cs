using BookIt.BLL.Services;
using Microsoft.AspNetCore.Mvc;

namespace BookIt.API.Controllers;

[Route("google-auth")]
[ApiController]
public class GoogleAuthController : ControllerBase
{
    private readonly GoogleAuthService _googleAuthService;
    private readonly UserService _userService;
    private readonly JWTService _jwtService;

    public GoogleAuthController(GoogleAuthService googleAuthService, UserService userService, JWTService jwtService)
    {
        _googleAuthService = googleAuthService;
        _userService = userService;
        _jwtService = jwtService;
    }

    [HttpGet("login")]
    public IActionResult Login()
    {
        var url = _googleAuthService.GetLoginUrl();
        return Redirect(url);
    }

    [HttpGet("callback")]
    public async Task<IActionResult> Callback([FromQuery] string code)
    {
        if (string.IsNullOrWhiteSpace(code))
            return BadRequest("Missing code");

        try
        {
            var (email, name) = await _googleAuthService.GetUserEmailAndNameAsync(code);
            var user = await _userService.AuthByGoogleAsync(name, email);


            var token = _jwtService.GenerateToken(user);
            return Ok(new
            {
                user.Id,
                user.Username,
                user.Email,
                user.Role,
                Token = token
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }
}