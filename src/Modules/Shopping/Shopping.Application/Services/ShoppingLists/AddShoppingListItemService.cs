using CoeurApi.Modules.Shopping.Application.Abstractions;
using CoeurApi.Modules.Shopping.Application.DTOs;
using CoeurApi.Modules.Shopping.Domain;
using CoeurApi.SharedKernel.Abstractions;

namespace CoeurApi.Modules.Shopping.Application.Services.ShoppingLists;

public class AddShoppingListItemService(GetOwnedShoppingListService getOwnedList, IShoppingListRepository repository, IUnitOfWork unitOfWork)
{
    public async Task<ListItemResponse> ExecuteAsync(Guid listId, AddListItemDto dto, Guid ownerId, CancellationToken cancellationToken = default)
    {
        var list = await getOwnedList.ExecuteAsync(listId, ownerId, cancellationToken);

        var item = ListItem.Create(listId, dto.Name, dto.Quantity, dto.Unit, dto.ProductId);
        await repository.AddItemAsync(item, cancellationToken);
        list.Touch();
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return ListItemResponse.FromEntity(item);
    }
}
