using Microsoft.AspNetCore.Mvc;
using CoeurApi.Application.Abstractions;
using CoeurApi.Modules.Authentication.Application.UseCases;

namespace CoeurApi.Modules.Authentication.Presentation.Controller;

[ApiController]
[Route("api/v1/auth")]
public class MeController(ICurrentUser user) : ControllerBase
{
    [HttpGet("me")]
    [EndpointSummary("Dados do usuário autenticado")]
    [EndpointDescription("Retorna id, nome e email do usuário dono do access_token enviado no cookie.")]
    [ProducesResponseType<MeResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    public ActionResult<MeResponse> Me()
    {
        return Ok(new MeResponse(user.Id, user.Name, user.Email));
    }
}
