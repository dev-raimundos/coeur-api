using CoeurApi.App.Core.Database;
using CoeurApi.App.Modules.Users.DTOs;
using CoeurApi.App.Modules.Users.Models;
using CoeurApi.App.Shared.Exceptions;
using CoeurApi.App.Shared.Interfaces;

namespace CoeurApi.App.Modules.Users.Services;

public class UsersService(IUsersRepository repository, AppDbContext context, ICurrentUser currentUser)
{
    private const string ErrNotFound = "Usuário não encontrado.";
    private const string ErrEmailInUse = "Email já está em uso.";

    public async Task<UserResponse> CreateAsync(CreateUserDto dto)
    {
        if (await repository.ExistsByEmailAsync(dto.Email))
            throw AppException.Conflict(ErrEmailInUse);

        var passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);
        var user = User.Create(dto.Name, dto.Email, passwordHash);

        await repository.AddAsync(user);
        await context.SaveChangesAsync();

        return UserResponse.FromEntity(user);
    }

    public async Task<UserResponse> GetByIdAsync(Guid id)
    {
        if (id != currentUser.Id && !currentUser.IsAdmin)
            throw AppException.Forbidden();

        var user = await repository.GetByIdAsync(id)
            ?? throw AppException.NotFound(ErrNotFound);

        return UserResponse.FromEntity(user);
    }

    public async Task<UserResponse> UpdateAsync(Guid id, UpdateUserDto dto)
    {
        if (id != currentUser.Id && !currentUser.IsAdmin)
            throw AppException.Forbidden();

        var user = await repository.GetByIdAsync(id) ?? throw AppException.NotFound(ErrNotFound);
        user.UpdateProfile(dto.Name);
        await context.SaveChangesAsync();

        return UserResponse.FromEntity(user);
    }

    public async Task DeleteAsync(Guid id)
    {
        if (id != currentUser.Id && !currentUser.IsAdmin)
            throw AppException.Forbidden();

        var user = await repository.GetByIdAsync(id) ?? throw AppException.NotFound(ErrNotFound);

        repository.Delete(user);
        await context.SaveChangesAsync();
    }
}
