using CoeurApi.Modules.Shopping.Application.UseCases.ShoppingLists;
using CoeurApi.Modules.Shopping.Application.UseCases.ShoppingLists.GetOwned;
using CoeurApi.SharedKernel.Abstractions;

namespace CoeurApi.Modules.Shopping.Application.UseCases.ShoppingLists.Update;

public class UpdateShoppingListUseCase(GetOwnedShoppingListUseCase getOwnedList, IUnitOfWork unitOfWork)
{
    public async Task<ShoppingListResponse> ExecuteAsync(Guid id, UpdateShoppingListRequest request, Guid ownerId, CancellationToken cancellationToken = default)
    {
        var list = await getOwnedList.ExecuteAsync(id, ownerId, cancellationToken);

        list.Rename(request.Name);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return ShoppingListResponse.FromEntity(list);
    }
}
