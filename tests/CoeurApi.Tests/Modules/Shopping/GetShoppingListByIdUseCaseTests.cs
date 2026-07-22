using CoeurApi.Modules.Shopping.Domain;
using CoeurApi.Modules.Shopping.Application.Abstractions;
using CoeurApi.Modules.Shopping.Application.UseCases.ShoppingLists;
using CoeurApi.SharedKernel.Exceptions;
using Moq;

namespace CoeurApi.Tests.Modules.Shopping;

public class GetShoppingListByIdUseCaseTests
{
    private readonly Mock<IShoppingListRepository> _repository = new();

    private GetShoppingListByIdUseCase CreateUseCase() => new(new GetOwnedShoppingListUseCase(_repository.Object));

    [Fact]
    public async Task ExecuteAsync_ListaDeOutroDono_DeveLancarForbidden()
    {
        var list = ShoppingList.Create("Mercado", Guid.NewGuid());
        _repository.Setup(r => r.GetByIdWithItemsAsync(list.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(list);

        var useCase = CreateUseCase();

        var ex = await Assert.ThrowsAsync<HttpException>(() => useCase.ExecuteAsync(list.Id, Guid.NewGuid()));

        Assert.Equal(403, ex.StatusCode);
    }

    [Fact]
    public async Task ExecuteAsync_ListaInexistente_DeveLancarNotFound()
    {
        _repository.Setup(r => r.GetByIdWithItemsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ShoppingList?)null);

        var useCase = CreateUseCase();

        var ex = await Assert.ThrowsAsync<HttpException>(() => useCase.ExecuteAsync(Guid.NewGuid(), Guid.NewGuid()));

        Assert.Equal(404, ex.StatusCode);
    }
}
