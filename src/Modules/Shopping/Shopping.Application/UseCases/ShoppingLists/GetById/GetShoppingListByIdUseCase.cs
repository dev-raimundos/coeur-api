using CoeurApi.Modules.Shopping.Application.UseCases.ShoppingLists;
using CoeurApi.Modules.Shopping.Application.UseCases.ShoppingLists.GetOwned;

namespace CoeurApi.Modules.Shopping.Application.UseCases.ShoppingLists.GetById;

public class GetShoppingListByIdUseCase(GetOwnedShoppingListUseCase getOwnedList)
{
    public async Task<ShoppingListResponse> ExecuteAsync(Guid id, Guid ownerId, CancellationToken cancellationToken = default)
    {
        var list = await getOwnedList.ExecuteAsync(id, ownerId, cancellationToken);
        return ShoppingListResponse.FromEntity(list);
    }
}
