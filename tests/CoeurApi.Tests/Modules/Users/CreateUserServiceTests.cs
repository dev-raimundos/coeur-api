using CoeurApi.Modules.Users.Application.DTOs;
using CoeurApi.Modules.Users.Domain;
using CoeurApi.Modules.Users.Application.Services;
using CoeurApi.SharedKernel.Exceptions;
using CoeurApi.Modules.Users.Application.Abstractions;
using CoeurApi.SharedKernel.Abstractions;
using Moq;

namespace CoeurApi.Tests.Modules.Users;

public class CreateUserServiceTests
{
    private readonly Mock<IUsersRepository> _repository = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();

    private CreateUserService CreateService() => new(_repository.Object, _unitOfWork.Object);

    [Fact]
    public async Task ExecuteAsync_ComEmailJaExistente_DeveLancarConflict()
    {
        _repository.Setup(r => r.ExistsByEmailAsync("existente@teste.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var service = CreateService();
        var dto = new CreateUserDto("Fulano", "existente@teste.com", "senha12345");

        var ex = await Assert.ThrowsAsync<HttpException>(() => service.ExecuteAsync(dto));

        Assert.Equal(409, ex.StatusCode);
    }

    [Fact]
    public async Task ExecuteAsync_ComDadosValidos_DeveCriarUsuarioESalvar()
    {
        _repository.Setup(r => r.ExistsByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var service = CreateService();
        var dto = new CreateUserDto("Fulano", "novo@teste.com", "senha12345");

        var result = await service.ExecuteAsync(dto);

        Assert.Equal("novo@teste.com", result.Email);
        _repository.Verify(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
