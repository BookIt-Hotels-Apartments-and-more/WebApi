using BookIt.BLL.Exceptions;
using BookIt.BLL.Interfaces;
using BookIt.DAL.Configuration.Settings;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace BookIt.API.Controllers;

[ApiController]
[Route("google-auth")]
public class GoogleAuthController : ControllerBase
{
    private readonly IJWTService _jwtService;
    private readonly IUserService _userService;
    private readonly IGoogleAuthService _googleAuthService;
    private readonly IOptions<GoogleOAuthSettings> _googleOauthSettingsOptions;

    public GoogleAuthController(
        IJWTService jwtService,
        IUserService userService,
        IGoogleAuthService googleAuthService,
        IOptions<GoogleOAuthSettings> options)
    {
        _jwtService = jwtService;
        _userService = userService;
        _googleAuthService = googleAuthService;
        _googleOauthSettingsOptions = options;
    }

    [HttpGet("login")]
    public IActionResult Login()
    {
        try
        {
            var loginUrl = _googleAuthService.GetLoginUrl();
            return Redirect(loginUrl);
        }
        catch
        {
            var clientUrl = _googleOauthSettingsOptions.Value.RedirectClientUri;
            return Redirect($"{clientUrl}/auth/error");
        }
    }

    [HttpGet("callback")]
    public async Task<IActionResult> Callback([FromQuery] string code)
    {
        var clientUrl = _googleOauthSettingsOptions.Value.RedirectClientUri;

        try
        {
            if (string.IsNullOrWhiteSpace(code)) return Redirect($"{clientUrl}/auth/error");
            var (email, name, imageUrl) = await _googleAuthService.GetUserInfoAsync(code);
            if (string.IsNullOrWhiteSpace(email)) return Redirect($"{clientUrl}/auth/error");
            var user = await _userService.AuthByGoogleAsync(name ?? string.Empty, email, imageUrl);
            if (user is null) return Redirect($"{clientUrl}/auth/error");
            var token = await _jwtService.GenerateToken(user);
            return Redirect($"{clientUrl}/auth/success?token={token}");
        }
        catch (BusinessRuleViolationException ex)
        {
            return Redirect($"{clientUrl}/auth/error?error={ex.Message}");
        }
        catch
        {
            return Redirect($"{clientUrl}/auth/error");
        }
    }
}