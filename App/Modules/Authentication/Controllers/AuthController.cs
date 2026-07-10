using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;
using CoeurApi.App.Core.Settings;
using CoeurApi.App.Modules.Authentication.DTOs;
using CoeurApi.App.Modules.Authentication.Services;

namespace CoeurApi.App.Modules.Authentication.Controllers;

[ApiController]
[Route("api/v1/auth")]
public class AuthController(LoginService loginService, IOptions<JwtSettings> jwtSettings) : ControllerBase
{
    [HttpPost("login")]
    [AllowAnonymous]
    [EnableRateLimiting("login")]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginDto dto, CancellationToken cancellationToken)
    {
        var result = await loginService.ExecuteAsync(dto, cancellationToken);

        var response = result.Response;
        var token = result.Token;

        // Strict funciona pois front (coeur.app.br) e API (api.coeur.app.br) são same-site
        // (mesmo domínio registrável) — se o front algum dia sair desse domínio, isso quebra
        // e precisa virar SameSite.None (com Secure=true obrigatório).
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
    public ActionResult Logout()
    {
        Response.Cookies.Delete("access_token");
        return NoContent();
    }
}
