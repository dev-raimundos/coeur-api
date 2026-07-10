using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;
using CoeurApi.App.Core.Settings;
using CoeurApi.App.Modules.Authentication.DTOs;
using CoeurApi.App.Modules.Authentication.Services;

namespace CoeurApi.App.Modules.Authentication.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(AuthService service, IOptions<JwtSettings> jwtSettings) : ControllerBase
{
    [HttpPost("login")]
    [AllowAnonymous]
    [EnableRateLimiting("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var result = await service.LoginAsync(dto);

        var response = result.Response;
        var token = result.Token;

        Response.Cookies.Append("access_token", token, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTimeOffset.UtcNow.AddHours(jwtSettings.Value.ExpirationHours)
        });

        return Ok(response);
    }

    [HttpPost("logout")]
    [AllowAnonymous]
    public IActionResult Logout()
    {
        Response.Cookies.Delete("access_token");
        return NoContent();
    }
}
