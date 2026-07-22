using CoeurApi.Application.Abstractions;
using CoeurApi.Modules.Users.Application.Abstractions;
using CoeurApi.SharedKernel.Abstractions;
using CoeurApi.SharedKernel.Exceptions;

namespace CoeurApi.Modules.Users.Application.UseCases;

public class UpdateUserUseCase(IUsersRepository repository, IUnitOfWork unitOfWork, ICurrentUser currentUser)
{
    private const string ErrNotFound = "Usuário não encontrado.";

    public async Task<UserResponse> ExecuteAsync(Guid id, UpdateUserRequest request, CancellationToken cancellationToken = default)
    {
        if (id != currentUser.Id && !currentUser.IsAdmin)
            throw HttpException.Forbidden();

        var user = await repository.GetByIdAsync(id, cancellationToken) ?? throw HttpException.NotFound(ErrNotFound);
        user.UpdateProfile(request.Name);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return UserResponse.FromEntity(user);
    }
}
