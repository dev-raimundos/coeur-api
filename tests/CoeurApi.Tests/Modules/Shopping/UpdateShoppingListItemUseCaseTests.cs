using CoeurApi.Modules.Shopping.Application.UseCases.ShoppingLists;
using CoeurApi.Modules.Shopping.Domain;
using CoeurApi.Modules.Shopping.Application.Abstractions;
using CoeurApi.SharedKernel.Exceptions;
using CoeurApi.SharedKernel.Abstractions;
using Moq;

namespace CoeurApi.Tests.Modules.Shopping;

public class UpdateShoppingListItemUseCaseTests
{
    private readonly Mock<IShoppingListRepository> _repository = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();

    private UpdateShoppingListItemUseCase CreateUseCase()
        => new(new GetOwnedShoppingListUseCase(_repository.Object), _repository.Object, _unitOfWork.Object);

    [Fact]
    public async Task ExecuteAsync_ItemNaoEncontrado_DeveLancarNotFound()
    {
        var ownerId = Guid.NewGuid();
        var list = ShoppingList.Create("Mercado", ownerId);

        _repository.Setup(r => r.GetByIdWithItemsAsync(list.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(list);
        _repository.Setup(r => r.GetItemAsync(list.Id, It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ListItem?)null);

        var useCase = CreateUseCase();
        var request = new UpdateShoppingListItemRequest(IsChecked: true);

        var ex = await Assert.ThrowsAsync<HttpException>(() => useCase.ExecuteAsync(list.Id, Guid.NewGuid(), request, ownerId));

        Assert.Equal(404, ex.StatusCode);
    }
}
