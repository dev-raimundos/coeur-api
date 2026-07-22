using CoeurApi.Modules.Authentication.Application.Abstractions;
using CoeurApi.Modules.Users.Application.Abstractions;
using CoeurApi.Modules.Users.Application.UseCases;
using CoeurApi.SharedKernel.Abstractions;
using CoeurApi.SharedKernel.Exceptions;

namespace CoeurApi.Modules.Authentication.Application.UseCases;

public class LoginUseCase(IUsersRepository repository, ITokenService tokenService, IUnitOfWork unitOfWork)
{
    private const string ErrInvalidCredentials = "Credenciais inválidas.";
    private const string ErrAccountLocked = "Conta bloqueada temporariamente. Tente novamente em alguns minutos.";
    private const string ErrAccountInactive = "Conta desativada.";

    // Hash BCrypt válido sem usuário correspondente — verificado mesmo quando o e-mail não
    // existe, pra manter o tempo de resposta constante e evitar enumeração via timing.
    private const string DummyHash = "$2a$11$CwTycUXWue0Thq9StjUM0uJ8vY.SEmR5AZlSZDPGGStLL55E1Wei.";

    public async Task<(AuthResponse Response, string Token)> ExecuteAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var user = await repository.GetByEmailAsync(request.Email, cancellationToken);

        bool passwordValid = BCrypt.Net.BCrypt.Verify(request.Password, user?.PasswordHash ?? DummyHash);

        if (user is null)
        {
            throw HttpException.Unauthorized(ErrInvalidCredentials);
        }

        if (!user.IsActive)
        {
            throw HttpException.Forbidden(ErrAccountInactive);
        }

        if (user.IsLocked)
        {
            throw HttpException.TooManyRequests(ErrAccountLocked);
        }

        if (!passwordValid)
        {
            user.RecordFailedLogin();
            await unitOfWork.SaveChangesAsync(cancellationToken);
            throw HttpException.Unauthorized(ErrInvalidCredentials);
        }

        user.RecordLogin();
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var token = tokenService.Generate(user);
        var response = new AuthResponse(UserResponse.FromEntity(user));

        return (response, token);
    }
}
