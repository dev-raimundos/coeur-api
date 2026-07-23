using CoeurApi.Modules.Users.Application.UseCases.Create;
using CoeurApi.SharedKernel.Exceptions;
using CoeurApi.Modules.Users.Application.Abstractions;
using CoeurApi.SharedKernel.Abstractions;
using Moq;
using CoeurApi.Modules.Users.Domain.Model;

namespace CoeurApi.Tests.Modules.Users;

public class CreateUserUseCaseTests
{
    private readonly Mock<IUsersRepository> _repository = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();

    private CreateUserUseCase CreateUseCase() => new(_repository.Object, _unitOfWork.Object);

    [Fact]
    public async Task ExecuteAsync_ComEmailJaExistente_DeveLancarConflict()
    {
        _repository.Setup(r => r.ExistsByEmailAsync("existente@teste.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var useCase = CreateUseCase();
        var request = new CreateUserRequest("Fulano", "existente@teste.com", "senha12345");

        var ex = await Assert.ThrowsAsync<HttpException>(() => useCase.ExecuteAsync(request));

        Assert.Equal(409, ex.StatusCode);
    }

    [Fact]
    public async Task ExecuteAsync_ComDadosValidos_DeveCriarUsuarioESalvar()
    {
        _repository.Setup(r => r.ExistsByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var useCase = CreateUseCase();
        var request = new CreateUserRequest("Fulano", "novo@teste.com", "senha12345");

        var result = await useCase.ExecuteAsync(request);

        Assert.Equal("novo@teste.com", result.Email);
        _repository.Verify(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
