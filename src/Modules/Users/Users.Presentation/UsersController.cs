using Microsoft.AspNetCore.Mvc;
using CoeurApi.Modules.Users.Application.UseCases;
using CoeurApi.Modules.Users.Application.UseCases.Create;
using CoeurApi.Modules.Users.Application.UseCases.Delete;
using CoeurApi.Modules.Users.Application.UseCases.GetById;
using CoeurApi.Modules.Users.Application.UseCases.Update;

namespace CoeurApi.Modules.Users.Presentation;

[ApiController]
[Route("api/v1/users")]
public class UsersController(
    CreateUserUseCase createUser,
    GetUserByIdUseCase getUserById,
    UpdateUserUseCase updateUser,
    DeleteUserUseCase deleteUser) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<UserResponse>> Create([FromBody] CreateUserRequest request,
        CancellationToken cancellationToken)
    {
        var user = await createUser.ExecuteAsync(request, cancellationToken);
        return Created($"api/v1/users/{user.Id}", user);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType<UserResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<UserResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var user = await getUserById.ExecuteAsync(id, cancellationToken);
        return Ok(user);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType<UserResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<UserResponse>> Update(Guid id, [FromBody] UpdateUserRequest request,
        CancellationToken cancellationToken)
    {
        var user = await updateUser.ExecuteAsync(id, request, cancellationToken);
        return Ok(user);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<ActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await deleteUser.ExecuteAsync(id, cancellationToken);
        return NoContent();
    }
}
