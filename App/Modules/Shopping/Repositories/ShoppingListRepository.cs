using Microsoft.EntityFrameworkCore;
using CoeurApi.App.Core.Database;
using CoeurApi.App.Modules.Shopping.Models;

namespace CoeurApi.App.Modules.Shopping.Repositories;

public class ShoppingListRepository(AppDbContext context) : IShoppingListRepository
{
    public async Task<List<ShoppingList>> GetAllByOwnerAsync(Guid ownerId)
        => await context.ShoppingLists
            .Where(l => l.OwnerId == ownerId)
            .OrderByDescending(l => l.UpdatedAt)
            .ToListAsync();

    public async Task<ShoppingList?> GetByIdWithItemsAsync(Guid id)
        => await context.ShoppingLists
            .Include(l => l.Items)
                .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(l => l.Id == id);

    public async Task<ListItem?> GetItemAsync(Guid listId, Guid itemId)
        => await context.ListItems
            .FirstOrDefaultAsync(i => i.ShoppingListId == listId && i.Id == itemId);

    public async Task AddAsync(ShoppingList list)
        => await context.ShoppingLists.AddAsync(list);

    public async Task AddItemAsync(ListItem item)
        => await context.ListItems.AddAsync(item);

    public void Delete(ShoppingList list)
        => context.ShoppingLists.Remove(list);

    public void DeleteItem(ListItem item)
        => context.ListItems.Remove(item);
}
