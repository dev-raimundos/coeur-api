using CoeurApi.Modules.Shopping.Application.Abstractions;
using CoeurApi.Modules.Shopping.Application.DTOs;
using CoeurApi.SharedKernel.Common;

namespace CoeurApi.Modules.Shopping.Application.Services.ShoppingLists;

public class GetAllShoppingListsService(IShoppingListRepository repository)
{
    public async Task<PagedResult<ShoppingListResponse>> ExecuteAsync(Guid ownerId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var (lists, totalCount) = await repository.GetAllByOwnerAsync(ownerId, page, pageSize, cancellationToken);
        return new PagedResult<ShoppingListResponse>(lists.Select(ShoppingListResponse.FromEntity).ToList(), page, pageSize, totalCount);
    }
}
