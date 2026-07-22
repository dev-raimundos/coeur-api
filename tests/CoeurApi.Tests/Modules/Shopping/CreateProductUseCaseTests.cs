using CoeurApi.Modules.Shopping.Application.DTOs;
using CoeurApi.Modules.Shopping.Domain;
using CoeurApi.Modules.Shopping.Application.Abstractions;
using CoeurApi.Modules.Shopping.Application.Services.Products;
using CoeurApi.SharedKernel.Abstractions;
using Moq;

namespace CoeurApi.Tests.Modules.Shopping;

public class CreateProductServiceTests
{
    private readonly Mock<IProductRepository> _repository = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();

    private CreateProductService CreateService() => new(_repository.Object, _unitOfWork.Object);

    [Fact]
    public async Task ExecuteAsync_DeveCriarProdutoESalvar()
    {
        var service = CreateService();
        var dto = new CreateProductDto("Feijão", "Mercearia");

        var result = await service.ExecuteAsync(dto);

        Assert.Equal("Feijão", result.Name);
        _repository.Verify(r => r.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
