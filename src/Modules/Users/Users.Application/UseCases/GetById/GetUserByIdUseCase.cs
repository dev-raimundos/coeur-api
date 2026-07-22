using CoeurApi.Application.Abstractions;
using CoeurApi.Modules.Users.Application.Abstractions;
using CoeurApi.Modules.Users.Application.UseCases;
using CoeurApi.SharedKernel.Exceptions;

namespace CoeurApi.Modules.Users.Application.UseCases.GetById;

public class GetUserByIdUseCase(IUsersRepository repository, ICurrentUser currentUser)
{
    private const string ErrNotFound = "Usuário não encontrado.";

    public async Task<UserResponse> ExecuteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        if (id != currentUser.Id && !currentUser.IsAdmin)
        {
            throw HttpException.Forbidden();
        }

        var user = await repository.GetByIdAsync(id, cancellationToken) ?? throw HttpException.NotFound(ErrNotFound);

        return UserResponse.FromEntity(user);
    }
}
