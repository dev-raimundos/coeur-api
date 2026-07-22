using CoeurApi.Modules.Shopping.Application.Abstractions;
using CoeurApi.Modules.Shopping.Domain;
using CoeurApi.SharedKernel.Abstractions;

namespace CoeurApi.Modules.Shopping.Application.UseCases.ShoppingLists;

public class CreateShoppingListUseCase(IShoppingListRepository repository, IUnitOfWork unitOfWork)
{
    public async Task<ShoppingListResponse> ExecuteAsync(CreateShoppingListRequest request, Guid ownerId, CancellationToken cancellationToken = default)
    {
        var list = ShoppingList.Create(request.Name, ownerId);
        await repository.AddAsync(list, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return ShoppingListResponse.FromEntity(list);
    }
}
