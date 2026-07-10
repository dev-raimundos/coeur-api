using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CoeurApi.App.Modules.Users.DTOs;
using CoeurApi.App.Modules.Users.Services;

namespace CoeurApi.App.Modules.Users.Controllers;

[ApiController]
[Route("api/v1/users")]
public class UsersController(
    CreateUserService createUser,
    GetUserByIdService getUserById,
    UpdateUserService updateUser,
    DeleteUserService deleteUser) : ControllerBase
{
    [HttpPost]
    [AllowAnonymous]
    public async Task<ActionResult<UserResponse>> Create([FromBody] CreateUserDto dto, CancellationToken cancellationToken)
    {
        var user = await createUser.ExecuteAsync(dto, cancellationToken);
        return Created($"api/v1/users/{user.Id}", user);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<UserResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var user = await getUserById.ExecuteAsync(id, cancellationToken);
        return Ok(user);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<UserResponse>> Update(Guid id, [FromBody] UpdateUserDto dto, CancellationToken cancellationToken)
    {
        var user = await updateUser.ExecuteAsync(id, dto, cancellationToken);
        return Ok(user);
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await deleteUser.ExecuteAsync(id, cancellationToken);
        return NoContent();
    }
}
