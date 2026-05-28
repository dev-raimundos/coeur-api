using NeonVertexApi.App.Core.Database;
using NeonVertexApi.App.Modules.Users.DTOs;
using NeonVertexApi.App.Modules.Users.Models;
using NeonVertexApi.App.Shared.Exceptions;
using NeonVertexApi.App.Shared.Interfaces;

namespace NeonVertexApi.App.Modules.Users.Services;

public class UsersService(IUsersRepository repository, AppDbContext context)
{
    private const string ErrNotFound = "Usuário não encontrado.";
    private const string ErrEmailInUse = "Email já está em uso.";

    public async Task<UserResponse> CreateAsync(CreateUserDto dto)
    {
        if (await repository.ExistsByEmailAsync(dto.Email)) 
        {
            throw AppException.Conflict(ErrEmailInUse);
        }

        var passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);
        var user = User.Create(dto.Name, dto.Email, passwordHash);

        await repository.AddAsync(user);
        await context.SaveChangesAsync();

        return UserResponse.FromEntity(user);
    }

    public async Task<UserResponse> GetByIdAsync(Guid id)
    {
        var user = await repository.GetByIdAsync(id) 
            ?? throw AppException.NotFound(ErrNotFound);

        return UserResponse.FromEntity(user);
    }

    public async Task<UserResponse> UpdateAsync(Guid id, UpdateUserDto dto)
    {
        var user = await repository.GetByIdAsync(id)
            ?? throw AppException.NotFound(ErrNotFound);

        user.UpdateProfile(dto.Name);
        await context.SaveChangesAsync();

        return UserResponse.FromEntity(user);
    }

    public async Task DeleteAsync(Guid id)
    {
        var user = await repository.GetByIdAsync(id)
            ?? throw AppException.NotFound(ErrNotFound);

        await repository.DeleteAsync(user);
        await context.SaveChangesAsync();
    }
}