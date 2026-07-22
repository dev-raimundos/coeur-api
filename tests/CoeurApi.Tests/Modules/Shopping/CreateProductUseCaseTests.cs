using CoeurApi.Modules.Shopping.Application.UseCases.Products.Create;
using CoeurApi.Modules.Shopping.Domain;
using CoeurApi.Modules.Shopping.Application.Abstractions;
using CoeurApi.SharedKernel.Abstractions;
using Moq;

namespace CoeurApi.Tests.Modules.Shopping;

public class CreateProductUseCaseTests
{
    private readonly Mock<IProductRepository> _repository = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();

    private CreateProductUseCase CreateUseCase() => new(_repository.Object, _unitOfWork.Object);

    [Fact]
    public async Task ExecuteAsync_DeveCriarProdutoESalvar()
    {
        var useCase = CreateUseCase();
        var request = new CreateProductRequest("Feijão", "Mercearia");

        var result = await useCase.ExecuteAsync(request);

        Assert.Equal("Feijão", result.Name);
        _repository.Verify(r => r.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
