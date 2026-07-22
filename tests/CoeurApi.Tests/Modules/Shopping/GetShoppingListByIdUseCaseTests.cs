using CoeurApi.Modules.Shopping.Domain;
using CoeurApi.Modules.Shopping.Application.Abstractions;
using CoeurApi.Modules.Shopping.Application.Services.ShoppingLists;
using CoeurApi.SharedKernel.Exceptions;
using Moq;

namespace CoeurApi.Tests.Modules.Shopping;

public class GetShoppingListByIdServiceTests
{
    private readonly Mock<IShoppingListRepository> _repository = new();

    private GetShoppingListByIdService CreateService() => new(new GetOwnedShoppingListService(_repository.Object));

    [Fact]
    public async Task ExecuteAsync_ListaDeOutroDono_DeveLancarForbidden()
    {
        var list = ShoppingList.Create("Mercado", Guid.NewGuid());
        _repository.Setup(r => r.GetByIdWithItemsAsync(list.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(list);

        var service = CreateService();

        var ex = await Assert.ThrowsAsync<HttpException>(() => service.ExecuteAsync(list.Id, Guid.NewGuid()));

        Assert.Equal(403, ex.StatusCode);
    }

    [Fact]
    public async Task ExecuteAsync_ListaInexistente_DeveLancarNotFound()
    {
        _repository.Setup(r => r.GetByIdWithItemsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ShoppingList?)null);

        var service = CreateService();

        var ex = await Assert.ThrowsAsync<HttpException>(() => service.ExecuteAsync(Guid.NewGuid(), Guid.NewGuid()));

        Assert.Equal(404, ex.StatusCode);
    }
}
