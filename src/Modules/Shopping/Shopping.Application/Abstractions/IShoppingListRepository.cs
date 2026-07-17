using CoeurApi.Modules.Shopping.Domain;

namespace CoeurApi.Modules.Shopping.Application.Abstractions;

public interface IShoppingListRepository
{
    Task<(List<ShoppingList> Items, int TotalCount)> GetAllByOwnerAsync(Guid ownerId, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<ShoppingList?> GetByIdWithItemsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ListItem?> GetItemAsync(Guid listId, Guid itemId, CancellationToken cancellationToken = default);
    Task AddAsync(ShoppingList list, CancellationToken cancellationToken = default);
    Task AddItemAsync(ListItem item, CancellationToken cancellationToken = default);
    void Delete(ShoppingList list);
    void DeleteItem(ListItem item);
}
