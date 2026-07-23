using CoeurApi.Modules.Users.Application.Abstractions;
using CoeurApi.Modules.Users.Application.UseCases;
using CoeurApi.Modules.Users.Domain;
using CoeurApi.SharedKernel.Abstractions;
using CoeurApi.SharedKernel.Exceptions;

namespace CoeurApi.Modules.Users.Application.UseCases.Create;

public class CreateUserUseCase(IUsersRepository repository, IUnitOfWork unitOfWork)
{
    private const string ErrEmailInUse = "Email já está em uso.";

    public async Task<UserResponse> ExecuteAsync(CreateUserRequest request, CancellationToken cancellationToken = default)
    {
        if (await repository.ExistsByEmailAsync(request.Email, cancellationToken))
            throw HttpException.Conflict(ErrEmailInUse);

        var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
        var user = User.Create(request.Name, request.Email, passwordHash);

        await repository.AddAsync(user, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return UserResponse.FromEntity(user);
    }
}
