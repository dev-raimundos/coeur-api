using CoeurApi.Modules.Shopping.Application.Abstractions;
using CoeurApi.Modules.Shopping.Application.UseCases.ShoppingLists.GetOwned;
using CoeurApi.SharedKernel.Abstractions;

namespace CoeurApi.Modules.Shopping.Application.UseCases.ShoppingLists.Delete;

public class DeleteShoppingListUseCase(GetOwnedShoppingListUseCase getOwnedList, IShoppingListRepository repository, IUnitOfWork unitOfWork)
{
    public async Task ExecuteAsync(Guid id, Guid ownerId, CancellationToken cancellationToken = default)
    {
        var list = await getOwnedList.ExecuteAsync(id, ownerId, cancellationToken);

        repository.Delete(list);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
