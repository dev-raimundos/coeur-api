using CoeurApi.Modules.Users.Application.Abstractions;
using CoeurApi.Modules.Users.Application.DTOs;
using CoeurApi.Modules.Users.Domain;
using CoeurApi.SharedKernel.Abstractions;
using CoeurApi.SharedKernel.Exceptions;

namespace CoeurApi.Modules.Users.Application.Services;

public class CreateUserService(IUsersRepository repository, IUnitOfWork unitOfWork)
{
    private const string ErrEmailInUse = "Email já está em uso.";

    public async Task<UserResponse> ExecuteAsync(CreateUserDto dto, CancellationToken cancellationToken = default)
    {
        if (await repository.ExistsByEmailAsync(dto.Email, cancellationToken))
            throw HttpException.Conflict(ErrEmailInUse);

        var passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);
        var user = User.Create(dto.Name, dto.Email, passwordHash);

        await repository.AddAsync(user, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return UserResponse.FromEntity(user);
    }
}
