using CoeurApi.Modules.Shopping.Domain;
using CoeurApi.Modules.Shopping.Application.Abstractions;
using CoeurApi.Modules.Shopping.Application.UseCases.Products;
using CoeurApi.SharedKernel.Exceptions;
using Moq;

namespace CoeurApi.Tests.Modules.Shopping;

public class GetProductByIdUseCaseTests
{
    private readonly Mock<IProductRepository> _repository = new();

    private GetProductByIdUseCase CreateUseCase() => new(_repository.Object);

    [Fact]
    public async Task ExecuteAsync_ProdutoNaoEncontrado_DeveLancarNotFound()
    {
        _repository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        var useCase = CreateUseCase();

        var ex = await Assert.ThrowsAsync<HttpException>(() => useCase.ExecuteAsync(Guid.NewGuid()));

        Assert.Equal(404, ex.StatusCode);
    }
}
