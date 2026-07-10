using Microsoft.AspNetCore.Mvc;
using CoeurApi.App.Shared.Interfaces;

namespace CoeurApi.App.Modules.Authentication.Controllers;

[ApiController]
[Route("api/auth")]
public class MeController(ICurrentUser user) : ControllerBase
{
    [HttpGet("me")]
    public IActionResult Me()
    {
        return Ok(new
        {
            user.Id,
            user.Name,
            user.Email,
        });

    }
}
