using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;
using CoeurApi.Modules.Authentication.Application.DTOs;
using CoeurApi.Modules.Authentication.Application.Services;
using CoeurApi.Modules.Authentication.Application.Settings;

namespace CoeurApi.Modules.Authentication.Presentation;

[ApiController]
[Route("api/v1/auth")]
public class AuthController(LoginService loginService, IOptions<JwtSettings> jwtSettings) : ControllerBase
{
    [HttpPost("login")]
    [AllowAnonymous]
    [EnableRateLimiting("login")]
    [ProducesResponseType<AuthResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status429TooManyRequests)]
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
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public ActionResult Logout()
    {
        Response.Cookies.Delete("access_token");
        return NoContent();
    }
}
