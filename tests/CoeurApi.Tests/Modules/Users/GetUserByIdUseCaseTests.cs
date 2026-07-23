using CoeurApi.Modules.Users.Application.Abstractions;
using CoeurApi.Modules.Users.Application.UseCases.GetById;
using CoeurApi.SharedKernel.Exceptions;
using CoeurApi.Application.Abstractions;
using Moq;
using CoeurApi.Modules.Users.Domain.Model;

namespace CoeurApi.Tests.Modules.Users;

public class GetUserByIdUseCaseTests
{
    private readonly Mock<IUsersRepository> _repository = new();
    private readonly Mock<ICurrentUser> _currentUser = new();

    private GetUserByIdUseCase CreateUseCase() => new(_repository.Object, _currentUser.Object);

    [Fact]
    public async Task ExecuteAsync_UsuarioTentandoAcessarOutroPerfil_DeveLancarForbidden()
    {
        _currentUser.Setup(c => c.Id).Returns(Guid.NewGuid());
        _currentUser.Setup(c => c.IsAdmin).Returns(false);

        var useCase = CreateUseCase();

        var ex = await Assert.ThrowsAsync<HttpException>(() => useCase.ExecuteAsync(Guid.NewGuid()));

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

        var useCase = CreateUseCase();
        var result = await useCase.ExecuteAsync(targetUser.Id);

        Assert.Equal(targetUser.Id, result.Id);
    }

    [Fact]
    public async Task ExecuteAsync_UsuarioNaoEncontrado_DeveLancarNotFound()
    {
        var id = Guid.NewGuid();
        _currentUser.Setup(c => c.Id).Returns(id);
        _repository.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var useCase = CreateUseCase();

        var ex = await Assert.ThrowsAsync<HttpException>(() => useCase.ExecuteAsync(id));

        Assert.Equal(404, ex.StatusCode);
    }
}
