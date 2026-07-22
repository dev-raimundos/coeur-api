using CoeurApi.Modules.Shopping.Application.Abstractions;
using CoeurApi.SharedKernel.Abstractions;
using CoeurApi.SharedKernel.Exceptions;

namespace CoeurApi.Modules.Shopping.Application.UseCases.ShoppingLists;

public class RemoveShoppingListItemUseCase(GetOwnedShoppingListUseCase getOwnedList, IShoppingListRepository repository, IUnitOfWork unitOfWork)
{
    private const string ErrItemNotFound = "Item não encontrado na lista.";

    public async Task ExecuteAsync(Guid listId, Guid itemId, Guid ownerId, CancellationToken cancellationToken = default)
    {
        var list = await getOwnedList.ExecuteAsync(listId, ownerId, cancellationToken);

        var item = await repository.GetItemAsync(listId, itemId, cancellationToken)
            ?? throw HttpException.NotFound(ErrItemNotFound);

        repository.DeleteItem(item);
        list.Touch();
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
