using CoeurApi.Application.Abstractions;
using CoeurApi.Modules.Users.Application.Abstractions;
using CoeurApi.SharedKernel.Abstractions;
using CoeurApi.SharedKernel.Exceptions;

namespace CoeurApi.Modules.Users.Application.UseCases.Delete;

public class DeleteUserUseCase(IUsersRepository repository, IUnitOfWork unitOfWork, ICurrentUser currentUser)
{
    private const string ErrNotFound = "Usuário não encontrado.";

    public async Task ExecuteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        if (id != currentUser.Id && !currentUser.IsAdmin)
            throw HttpException.Forbidden();

        var user = await repository.GetByIdAsync(id, cancellationToken) ?? throw HttpException.NotFound(ErrNotFound);

        repository.Delete(user);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
