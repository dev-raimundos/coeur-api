using Microsoft.AspNetCore.Mvc;
using CoeurApi.App.Modules.Authentication.DTOs;
using CoeurApi.App.Shared.Interfaces;

namespace CoeurApi.App.Modules.Authentication.Controllers;

[ApiController]
[Route("api/v1/auth")]
public class MeController(ICurrentUser user) : ControllerBase
{
    [HttpGet("me")]
    public ActionResult<MeResponse> Me()
    {
        return Ok(new MeResponse(user.Id, user.Name, user.Email));
    }
}
