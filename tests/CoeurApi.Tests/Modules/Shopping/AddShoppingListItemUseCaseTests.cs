using CoeurApi.Modules.Shopping.Application.UseCases.ShoppingLists.AddItem;
using CoeurApi.Modules.Shopping.Application.UseCases.ShoppingLists.GetOwned;
using CoeurApi.Modules.Shopping.Domain;
using CoeurApi.Modules.Shopping.Application.Abstractions;
using CoeurApi.SharedKernel.Abstractions;
using Moq;

namespace CoeurApi.Tests.Modules.Shopping;

public class AddShoppingListItemUseCaseTests
{
    private readonly Mock<IShoppingListRepository> _repository = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();

    private AddShoppingListItemUseCase CreateUseCase()
        => new(new GetOwnedShoppingListUseCase(_repository.Object), _repository.Object, _unitOfWork.Object);

    [Fact]
    public async Task ExecuteAsync_DeveAdicionarItemNaListaDoDono()
    {
        var ownerId = Guid.NewGuid();
        var list = ShoppingList.Create("Mercado", ownerId);

        _repository.Setup(r => r.GetByIdWithItemsAsync(list.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(list);

        var useCase = CreateUseCase();
        var request = new AddShoppingListItemRequest("Leite", 2, "un");

        var result = await useCase.ExecuteAsync(list.Id, request, ownerId);

        Assert.Equal("Leite", result.Name);
        Assert.Equal(2, result.Quantity);
        _repository.Verify(r => r.AddItemAsync(It.IsAny<ListItem>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
