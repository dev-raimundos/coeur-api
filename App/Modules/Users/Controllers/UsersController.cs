using Microsoft.AspNetCore.Mvc;
using NeonVertexApi.App.Modules.Users.DTOs;
using NeonVertexApi.App.Modules.Users.Services;

namespace NeonVertexApi.App.Modules.Users.Controllers;

[ApiController]
[Route("api/users")]
public class UsersController(UsersService service) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateUserDto dto)
    {
        var result = await service.CreateAsync(dto);

        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });

        return Created($"api/users/{result.Data!.Id}", result.Data);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await service.GetByIdAsync(id);

        if (!result.IsSuccess)
            return NotFound(new { error = result.Error });

        return Ok(result.Data);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateUserDto dto)
    {
        var result = await service.UpdateAsync(id, dto);

        if (!result.IsSuccess)
            return NotFound(new { error = result.Error });

        return Ok(result.Data);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await service.DeleteAsync(id);

        if (!result.IsSuccess)
            return NotFound(new { error = result.Error });

        return NoContent();
    }
}