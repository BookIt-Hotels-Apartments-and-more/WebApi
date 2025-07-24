using BookIt.BLL.Services;
using BookIt.DAL.Configuration.Settings;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace BookIt.API.Controllers;

[Route("google-auth")]
[ApiController]
public class GoogleAuthController : ControllerBase
{
    private readonly IGoogleAuthService _googleAuthService;
    private readonly IUserService _userService;
    private readonly IJWTService _jwtService;
    private readonly IOptions<UrlSettings> _urlSettingsOptions;

    public GoogleAuthController(
        IJWTService jwtService,
        IUserService userService,
        IGoogleAuthService googleAuthService,
        IOptions<UrlSettings> urlSettingsOptions)
    {
        _jwtService = jwtService;
        _userService = userService;
        _googleAuthService = googleAuthService;
        _urlSettingsOptions = urlSettingsOptions;
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

        var clientUrl = _urlSettingsOptions.Value.ClientUrl;

        try
        {
            var (email, name) = await _googleAuthService.GetUserEmailAndNameAsync(code);
            var user = await _userService.AuthByGoogleAsync(name, email);

            if (user == null)
            {
                return Redirect($"{clientUrl}/auth/error?error=User is nullable");
            }

            var token = _jwtService.GenerateToken(user);

            return Redirect($"{clientUrl}/auth/success?token={token}");
        }
        catch (Exception ex)
        {
            return Redirect($"{clientUrl}/auth/error?error={ex.Message}");
        }
    }
}