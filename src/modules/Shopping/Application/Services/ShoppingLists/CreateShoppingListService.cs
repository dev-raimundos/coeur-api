using CoeurApi.Modules.Shopping.Application.Abstractions;
using CoeurApi.Modules.Shopping.Application.DTOs;
using CoeurApi.Modules.Shopping.Domain;
using CoeurApi.SharedKernel.Abstractions;

namespace CoeurApi.Modules.Shopping.Application.Services.ShoppingLists;

public class CreateShoppingListService(IShoppingListRepository repository, IUnitOfWork unitOfWork)
{
    public async Task<ShoppingListResponse> ExecuteAsync(CreateShoppingListDto dto, Guid ownerId, CancellationToken cancellationToken = default)
    {
        var list = ShoppingList.Create(dto.Name, ownerId);
        await repository.AddAsync(list, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return ShoppingListResponse.FromEntity(list);
    }
}
