using CoeurApi.Modules.Shopping.Application.DTOs;
using CoeurApi.SharedKernel.Abstractions;

namespace CoeurApi.Modules.Shopping.Application.Services.ShoppingLists;

public class UpdateShoppingListService(GetOwnedShoppingListService getOwnedList, IUnitOfWork unitOfWork)
{
    public async Task<ShoppingListResponse> ExecuteAsync(Guid id, UpdateShoppingListDto dto, Guid ownerId, CancellationToken cancellationToken = default)
    {
        var list = await getOwnedList.ExecuteAsync(id, ownerId, cancellationToken);

        list.Rename(dto.Name);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return ShoppingListResponse.FromEntity(list);
    }
}
