using NeonVertexApi.App.Core.Database;
using NeonVertexApi.App.Modules.Users.DTOs;
using NeonVertexApi.App.Modules.Users.Models;
using NeonVertexApi.App.Modules.Users.Repositories;
using NeonVertexApi.App.Shared.Interfaces;
using NeonVertexApi.App.Shared.Models;

namespace NeonVertexApi.App.Modules.Users.Services;

public class UsersService(IUsersRepository repository, AppDbContext context)
{
    public async Task<Result<UserResponse>> CreateAsync(CreateUserDto dto)
    {
        if (await repository.ExistsByEmailAsync(dto.Email))
            return Result<UserResponse>.Failure("Email já está em uso.");

        var passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);
        var user = User.Create(dto.Name, dto.Email, passwordHash);

        await repository.AddAsync(user);
        await context.SaveChangesAsync();

        return Result<UserResponse>.Success(MapToResponse(user));
    }

    public async Task<Result<UserResponse>> GetByIdAsync(Guid id)
    {
        var user = await repository.GetByIdAsync(id);

        if (user is null)
            return Result<UserResponse>.Failure("Usuário não encontrado.");

        return Result<UserResponse>.Success(MapToResponse(user));
    }

    public async Task<Result<UserResponse>> UpdateAsync(Guid id, UpdateUserDto dto)
    {
        var user = await repository.GetByIdAsync(id);

        if (user is null)
            return Result<UserResponse>.Failure("Usuário não encontrado.");

        user.UpdateProfile(dto.Name);
        await context.SaveChangesAsync();

        return Result<UserResponse>.Success(MapToResponse(user));
    }

    public async Task<Result> DeleteAsync(Guid id)
    {
        var user = await repository.GetByIdAsync(id);

        if (user is null)
            return Result.Failure("Usuário não encontrado.");

        await repository.DeleteAsync(user);
        await context.SaveChangesAsync();

        return Result.Success();
    }

    private static UserResponse MapToResponse(User user) => new(
        user.Id,
        user.Name,
        user.Email,
        user.IsActive,
        user.IsEmailVerified,
        user.CreatedAt,
        user.UpdatedAt,
        user.LastLoginAt
    );
}