using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CoeurApi.App.Modules.Users.DTOs;
using CoeurApi.App.Modules.Users.Services;

namespace CoeurApi.App.Modules.Users.Controllers;

[ApiController]
[Route("api/users")]
public class UsersController(UsersService service) : ControllerBase
{
    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> Create([FromBody] CreateUserDto dto)
    {
        var user = await service.CreateAsync(dto);
        return Created($"api/users/{user.Id}", user);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var user = await service.GetByIdAsync(id);
        return Ok(user);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateUserDto dto)
    {
        var user = await service.UpdateAsync(id, dto);
        return Ok(user);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await service.DeleteAsync(id);
        return NoContent();
    }
}
