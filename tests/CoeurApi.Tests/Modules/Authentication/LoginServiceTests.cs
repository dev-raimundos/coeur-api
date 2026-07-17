using CoeurApi.Modules.Authentication.Infrastructure.Security;
using CoeurApi.Modules.Authentication.Application.Settings;
using CoeurApi.Modules.Authentication.Application.DTOs;
using CoeurApi.Modules.Authentication.Application.Services;
using CoeurApi.Modules.Users.Domain;
using CoeurApi.SharedKernel.Exceptions;
using CoeurApi.Modules.Users.Application.Abstractions;
using CoeurApi.SharedKernel.Abstractions;
using Microsoft.Extensions.Options;
using Moq;

namespace CoeurApi.Tests.Modules.Authentication;

public class LoginServiceTests
{
    private readonly Mock<IUsersRepository> _repository = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();

    private readonly TokenService _tokenService = new(Options.Create(new JwtSettings
    {
        Secret = "chave-de-teste-com-pelo-menos-32-caracteres",
        Issuer = "coeur-api-tests",
        Audience = "coeur-api-tests",
        ExpirationHours = 1
    }));

    private LoginService CreateService() => new(_repository.Object, _tokenService, _unitOfWork.Object);

    private static User CreateActiveUser(string password = "senha-correta")
        => User.Create("Fulano", "fulano@teste.com", BCrypt.Net.BCrypt.HashPassword(password));

    [Fact]
    public async Task ExecuteAsync_ComEmailInexistente_DeveLancarUnauthorized()
    {
        _repository.Setup(r => r.GetByEmailAsync("naoexiste@teste.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var service = CreateService();
        var dto = new LoginDto("naoexiste@teste.com", "qualquer-senha");

        var ex = await Assert.ThrowsAsync<HttpException>(() => service.ExecuteAsync(dto));

        Assert.Equal(401, ex.StatusCode);
    }

    [Fact]
    public async Task ExecuteAsync_ComSenhaErrada_DeveIncrementarTentativasELancarUnauthorized()
    {
        var user = CreateActiveUser();
        _repository.Setup(r => r.GetByEmailAsync(user.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var service = CreateService();
        var dto = new LoginDto(user.Email, "senha-errada");

        var ex = await Assert.ThrowsAsync<HttpException>(() => service.ExecuteAsync(dto));

        Assert.Equal(401, ex.StatusCode);
        Assert.Equal(1, user.FailedLoginAttempts);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_ComContaBloqueada_DeveLancarTooManyRequests()
    {
        var user = CreateActiveUser();
        for (var i = 0; i < 5; i++)
            user.RecordFailedLogin();

        _repository.Setup(r => r.GetByEmailAsync(user.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var service = CreateService();
        var dto = new LoginDto(user.Email, "senha-correta");

        var ex = await Assert.ThrowsAsync<HttpException>(() => service.ExecuteAsync(dto));

        Assert.Equal(429, ex.StatusCode);
    }

    [Fact]
    public async Task ExecuteAsync_ComCredenciaisValidas_DeveRetornarTokenERegistrarLogin()
    {
        var user = CreateActiveUser();
        _repository.Setup(r => r.GetByEmailAsync(user.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var service = CreateService();
        var dto = new LoginDto(user.Email, "senha-correta");

        var (response, token) = await service.ExecuteAsync(dto);

        Assert.False(string.IsNullOrWhiteSpace(token));
        Assert.Equal(user.Email, response.User.Email);
        Assert.Equal(0, user.FailedLoginAttempts);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
