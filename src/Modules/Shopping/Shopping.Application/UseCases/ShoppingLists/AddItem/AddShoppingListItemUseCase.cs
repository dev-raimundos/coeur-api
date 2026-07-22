using CoeurApi.Modules.Shopping.Application.Abstractions;
using CoeurApi.Modules.Shopping.Application.UseCases.ShoppingLists;
using CoeurApi.Modules.Shopping.Application.UseCases.ShoppingLists.GetOwned;
using CoeurApi.Modules.Shopping.Domain;
using CoeurApi.SharedKernel.Abstractions;

namespace CoeurApi.Modules.Shopping.Application.UseCases.ShoppingLists.AddItem;

public class AddShoppingListItemUseCase(GetOwnedShoppingListUseCase getOwnedList, IShoppingListRepository repository, IUnitOfWork unitOfWork)
{
    public async Task<ListItemResponse> ExecuteAsync(Guid listId, AddShoppingListItemRequest request, Guid ownerId, CancellationToken cancellationToken = default)
    {
        var list = await getOwnedList.ExecuteAsync(listId, ownerId, cancellationToken);

        var item = ListItem.Create(listId, request.Name, request.Quantity, request.Unit, request.ProductId);
        await repository.AddItemAsync(item, cancellationToken);
        list.Touch();
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return ListItemResponse.FromEntity(item);
    }
}
