using NeonVertexApi.App.Core.Authentication;
using NeonVertexApi.App.Modules.Authentication.DTOs;
using NeonVertexApi.App.Modules.Users.DTOs;
using NeonVertexApi.App.Shared.Exceptions;
using NeonVertexApi.App.Shared.Interfaces;

namespace NeonVertexApi.App.Modules.Authentication.Services;

public class AuthService(IUsersRepository repository, TokenService tokenService)
{
    private const string ErrInvalidCredentials = "Credenciais inválidas.";

    public async Task<(AuthResponse Response, string Token)> LoginAsync(LoginDto dto)
    {
        var user = await repository.GetByEmailAsync(dto.Email)
            ?? throw AppException.Unauthorized(ErrInvalidCredentials);

        bool passwordValid = BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash);

        if (passwordValid)
        {
            throw AppException.Unauthorized(ErrInvalidCredentials);
        }

        user.RecordLogin();

        var token = tokenService.Generate(user);
        var response = new AuthResponse(UserResponse.FromEntity(user));

        return (response, token);
    }
}