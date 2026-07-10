using CoeurApi.App.Modules.Shopping.Models;

namespace CoeurApi.App.Modules.Shopping.Repositories;

public interface IShoppingListRepository
{
    Task<List<ShoppingList>> GetAllByOwnerAsync(Guid ownerId);
    Task<ShoppingList?> GetByIdWithItemsAsync(Guid id);
    Task<ListItem?> GetItemAsync(Guid listId, Guid itemId);
    Task AddAsync(ShoppingList list);
    Task AddItemAsync(ListItem item);
    void Delete(ShoppingList list);
    void DeleteItem(ListItem item);
}
