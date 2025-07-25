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
        try
        {
            var url = _googleAuthService.GetLoginUrl();
            return Redirect(url);
        }
        catch (Exception)
        {
            var clientUrl = _urlSettingsOptions.Value.ClientUrl;
            return Redirect($"{clientUrl}/auth/error");
        }
    }

    [HttpGet("callback")]
    public async Task<IActionResult> Callback([FromQuery] string code)
    {
        var clientUrl = _urlSettingsOptions.Value.ClientUrl;

        try
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                return Redirect($"{clientUrl}/auth/error");
            }

            var (email, name) = await _googleAuthService.GetUserEmailAndNameAsync(code);

            if (string.IsNullOrWhiteSpace(email))
            {
                return Redirect($"{clientUrl}/auth/error");
            }

            var user = await _userService.AuthByGoogleAsync(name ?? string.Empty, email);

            if (user == null)
            {
                return Redirect($"{clientUrl}/auth/error");
            }

            var token = _jwtService.GenerateToken(user);

            Response.Cookies.Append("auth_token", token, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Lax,
                MaxAge = TimeSpan.FromHours(24)
            });

            return Redirect($"{clientUrl}/auth/success");
        }
        catch (Exception)
        {
            return Redirect($"{clientUrl}/auth/error");
        }
    }
}