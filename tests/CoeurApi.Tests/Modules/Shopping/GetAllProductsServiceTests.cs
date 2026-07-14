using CoeurApi.Modules.Shopping.Domain;
using CoeurApi.Modules.Shopping.Application.Abstractions;
using CoeurApi.Modules.Shopping.Application.Services.Products;
using Moq;

namespace CoeurApi.Tests.Modules.Shopping;

public class GetAllProductsServiceTests
{
    private readonly Mock<IProductRepository> _repository = new();

    private GetAllProductsService CreateService() => new(_repository.Object);

    [Fact]
    public async Task ExecuteAsync_DeveRetornarPagedResultComTotalCount()
    {
        var products = new List<Product> { Product.Create("Arroz", "Mercearia") };
        _repository.Setup(r => r.GetAllAsync(null, 1, 20, It.IsAny<CancellationToken>()))
            .ReturnsAsync((products, 1));

        var service = CreateService();
        var result = await service.ExecuteAsync(null, 1, 20);

        Assert.Single(result.Items);
        Assert.Equal(1, result.TotalCount);
        Assert.Equal(1, result.Page);
        Assert.Equal(20, result.PageSize);
    }
}
