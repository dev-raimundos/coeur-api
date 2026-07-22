using CoeurApi.Modules.Shopping.Application.Abstractions;
using CoeurApi.Modules.Shopping.Application.UseCases.ShoppingLists;
using CoeurApi.Modules.Shopping.Application.UseCases.ShoppingLists.GetOwned;
using CoeurApi.SharedKernel.Abstractions;
using CoeurApi.SharedKernel.Exceptions;

namespace CoeurApi.Modules.Shopping.Application.UseCases.ShoppingLists.UpdateItem;

public class UpdateShoppingListItemUseCase(GetOwnedShoppingListUseCase getOwnedList, IShoppingListRepository repository, IUnitOfWork unitOfWork)
{
    private const string ErrItemNotFound = "Item não encontrado na lista.";

    public async Task<ListItemResponse> ExecuteAsync(Guid listId, Guid itemId, UpdateShoppingListItemRequest request, Guid ownerId, CancellationToken cancellationToken = default)
    {
        var list = await getOwnedList.ExecuteAsync(listId, ownerId, cancellationToken);

        var item = await repository.GetItemAsync(listId, itemId, cancellationToken)
            ?? throw HttpException.NotFound(ErrItemNotFound);

        if (request.IsChecked is true) item.Check();
        else if (request.IsChecked is false) item.Uncheck();

        if (request.Name is not null || request.Quantity is not null || request.Unit is not null)
            item.UpdateDetails(request.Name ?? item.Name, request.Quantity ?? item.Quantity, request.Unit ?? item.Unit);

        list.Touch();
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return ListItemResponse.FromEntity(item);
    }
}
