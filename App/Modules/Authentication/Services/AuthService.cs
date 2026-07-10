using CoeurApi.App.Core.Authentication;
using CoeurApi.App.Core.Database;
using CoeurApi.App.Modules.Authentication.DTOs;
using CoeurApi.App.Modules.Users.DTOs;
using CoeurApi.App.Shared.Exceptions;
using CoeurApi.App.Shared.Interfaces;

namespace CoeurApi.App.Modules.Authentication.Services;

public class AuthService(IUsersRepository repository, TokenService tokenService, AppDbContext context)
{
    private const string ErrInvalidCredentials = "Credenciais inválidas.";
    private const string ErrAccountLocked = "Conta bloqueada temporariamente. Tente novamente em alguns minutos.";
    private const string ErrAccountInactive = "Conta desativada.";

    public async Task<(AuthResponse Response, string Token)> LoginAsync(LoginDto dto)
    {
        var user = await repository.GetByEmailAsync(dto.Email)
            ?? throw AppException.Unauthorized(ErrInvalidCredentials);

        if (!user.IsActive)
            throw AppException.Forbidden(ErrAccountInactive);

        if (user.IsLocked)
            throw AppException.TooManyRequests(ErrAccountLocked);

        bool passwordValid = BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash);

        if (!passwordValid)
        {
            user.RecordFailedLogin();
            await context.SaveChangesAsync();
            throw AppException.Unauthorized(ErrInvalidCredentials);
        }

        user.RecordLogin();
        await context.SaveChangesAsync();

        var token = tokenService.Generate(user);
        var response = new AuthResponse(UserResponse.FromEntity(user));

        return (response, token);
    }
}
