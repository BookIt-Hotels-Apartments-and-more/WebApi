using BookIt.BLL.Services;
using Microsoft.AspNetCore.Mvc;

namespace BookIt.API.Controllers;

[Route("google-auth")]
[ApiController]
public class GoogleAuthController : ControllerBase
{
    private readonly IGoogleAuthService _googleAuthService;
    private readonly IUserService _userService;
    private readonly IJWTService _jwtService;
    private readonly string _clientUrl;

    public GoogleAuthController(
        IGoogleAuthService googleAuthService,
        IUserService userService,
        IJWTService jwtService,
        IConfiguration configuration)
    {
        _googleAuthService = googleAuthService;
        _userService = userService;
        _jwtService = jwtService;
        _clientUrl = configuration.GetValue<string>("Urls:ClientUrl")
    ?? throw new InvalidOperationException("Redirect URL is not configured");
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

            if (user == null)
            {
                return Redirect($"{_clientUrl}/auth/error?error=User is nullable");
            }

            var token = _jwtService.GenerateToken(user);

            return Redirect($"{_clientUrl}/auth/success?token={token}");
        }
        catch (Exception ex)
        {
            return Redirect($"{_clientUrl}/auth/error?error={ex.Message}");
        }
    }
}