using NeonVertexApi.App.Core.Authentication;
using NeonVertexApi.App.Core.Database;
using NeonVertexApi.App.Modules.Authentication.DTOs;
using NeonVertexApi.App.Modules.Users.DTOs;
using NeonVertexApi.App.Shared.Exceptions;
using NeonVertexApi.App.Shared.Interfaces;

namespace NeonVertexApi.App.Modules.Authentication.Services;

public class AuthService(IUsersRepository repository, TokenService tokenService, AppDbContext context)
{
    private const string ErrInvalidCredentials = "Credenciais inválidas.";
    private const string ErrAccountLocked = "Conta bloqueada temporariamente. Tente novamente em alguns minutos.";

    public async Task<(AuthResponse Response, string Token)> LoginAsync(LoginDto dto)
    {
        var user = await repository.GetByEmailAsync(dto.Email)
            ?? throw AppException.Unauthorized(ErrInvalidCredentials);

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
