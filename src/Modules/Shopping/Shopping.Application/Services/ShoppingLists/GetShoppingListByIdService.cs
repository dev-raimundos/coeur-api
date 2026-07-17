using CoeurApi.Modules.Shopping.Application.DTOs;

namespace CoeurApi.Modules.Shopping.Application.Services.ShoppingLists;

public class GetShoppingListByIdService(GetOwnedShoppingListService getOwnedList)
{
    public async Task<ShoppingListResponse> ExecuteAsync(Guid id, Guid ownerId, CancellationToken cancellationToken = default)
    {
        var list = await getOwnedList.ExecuteAsync(id, ownerId, cancellationToken);
        return ShoppingListResponse.FromEntity(list);
    }
}
