using CoeurApi.Modules.Shopping.Application.Abstractions;
using CoeurApi.Modules.Shopping.Domain;
using CoeurApi.SharedKernel.Exceptions;

namespace CoeurApi.Modules.Shopping.Application.UseCases.ShoppingLists.GetOwned;

// Carrega uma lista garantindo que pertence ao dono informado — reaproveitado pelos
// demais use cases que precisam desse mesmo checkout de posse antes de agir sobre a lista.
public class GetOwnedShoppingListUseCase(IShoppingListRepository repository)
{
    private const string ErrListNotFound = "Lista de compras não encontrada.";

    public async Task<ShoppingList> ExecuteAsync(Guid id, Guid ownerId, CancellationToken cancellationToken = default)
    {
        var list = await repository.GetByIdWithItemsAsync(id, cancellationToken)
            ?? throw HttpException.NotFound(ErrListNotFound);

        if (list.OwnerId != ownerId)
            throw HttpException.Forbidden();

        return list;
    }
}
