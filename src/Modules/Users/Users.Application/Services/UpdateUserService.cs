using CoeurApi.Application.Abstractions;
using CoeurApi.Modules.Users.Application.Abstractions;
using CoeurApi.Modules.Users.Application.DTOs;
using CoeurApi.SharedKernel.Abstractions;
using CoeurApi.SharedKernel.Exceptions;

namespace CoeurApi.Modules.Users.Application.Services;

public class UpdateUserService(IUsersRepository repository, IUnitOfWork unitOfWork, ICurrentUser currentUser)
{
    private const string ErrNotFound = "Usuário não encontrado.";

    public async Task<UserResponse> ExecuteAsync(Guid id, UpdateUserDto dto, CancellationToken cancellationToken = default)
    {
        if (id != currentUser.Id && !currentUser.IsAdmin)
            throw HttpException.Forbidden();

        var user = await repository.GetByIdAsync(id, cancellationToken) ?? throw HttpException.NotFound(ErrNotFound);
        user.UpdateProfile(dto.Name);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return UserResponse.FromEntity(user);
    }
}
