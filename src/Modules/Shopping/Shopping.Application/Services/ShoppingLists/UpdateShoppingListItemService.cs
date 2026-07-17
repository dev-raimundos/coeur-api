using CoeurApi.Modules.Shopping.Application.Abstractions;
using CoeurApi.Modules.Shopping.Application.DTOs;
using CoeurApi.SharedKernel.Abstractions;
using CoeurApi.SharedKernel.Exceptions;

namespace CoeurApi.Modules.Shopping.Application.Services.ShoppingLists;

public class UpdateShoppingListItemService(GetOwnedShoppingListService getOwnedList, IShoppingListRepository repository, IUnitOfWork unitOfWork)
{
    private const string ErrItemNotFound = "Item não encontrado na lista.";

    public async Task<ListItemResponse> ExecuteAsync(Guid listId, Guid itemId, UpdateListItemDto dto, Guid ownerId, CancellationToken cancellationToken = default)
    {
        var list = await getOwnedList.ExecuteAsync(listId, ownerId, cancellationToken);

        var item = await repository.GetItemAsync(listId, itemId, cancellationToken)
            ?? throw HttpException.NotFound(ErrItemNotFound);

        if (dto.IsChecked is true) item.Check();
        else if (dto.IsChecked is false) item.Uncheck();

        if (dto.Name is not null || dto.Quantity is not null || dto.Unit is not null)
            item.UpdateDetails(dto.Name ?? item.Name, dto.Quantity ?? item.Quantity, dto.Unit ?? item.Unit);

        list.Touch();
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return ListItemResponse.FromEntity(item);
    }
}
