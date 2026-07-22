using CoeurApi.Modules.Users.Application.Abstractions;
using CoeurApi.Modules.Users.Domain;
using CoeurApi.Modules.Users.Application.Services;
using CoeurApi.SharedKernel.Exceptions;
using CoeurApi.Application.Abstractions;
using Moq;

namespace CoeurApi.Tests.Modules.Users;

public class GetUserByIdServiceTests
{
    private readonly Mock<IUsersRepository> _repository = new();
    private readonly Mock<ICurrentUser> _currentUser = new();

    private GetUserByIdService CreateService() => new(_repository.Object, _currentUser.Object);

    [Fact]
    public async Task ExecuteAsync_UsuarioTentandoAcessarOutroPerfil_DeveLancarForbidden()
    {
        _currentUser.Setup(c => c.Id).Returns(Guid.NewGuid());
        _currentUser.Setup(c => c.IsAdmin).Returns(false);

        var service = CreateService();

        var ex = await Assert.ThrowsAsync<HttpException>(() => service.ExecuteAsync(Guid.NewGuid()));

        Assert.Equal(403, ex.StatusCode);
    }

    [Fact]
    public async Task ExecuteAsync_Admin_DevePermitirAcessarQualquerUsuario()
    {
        var targetUser = User.Create("Fulano", "fulano@teste.com", "hash");
        _currentUser.Setup(c => c.Id).Returns(Guid.NewGuid());
        _currentUser.Setup(c => c.IsAdmin).Returns(true);
        _repository.Setup(r => r.GetByIdAsync(targetUser.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(targetUser);

        var service = CreateService();
        var result = await service.ExecuteAsync(targetUser.Id);

        Assert.Equal(targetUser.Id, result.Id);
    }

    [Fact]
    public async Task ExecuteAsync_UsuarioNaoEncontrado_DeveLancarNotFound()
    {
        var id = Guid.NewGuid();
        _currentUser.Setup(c => c.Id).Returns(id);
        _repository.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var service = CreateService();

        var ex = await Assert.ThrowsAsync<HttpException>(() => service.ExecuteAsync(id));

        Assert.Equal(404, ex.StatusCode);
    }
}
