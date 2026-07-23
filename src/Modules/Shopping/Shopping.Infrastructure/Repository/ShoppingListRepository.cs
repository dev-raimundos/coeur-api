using Microsoft.EntityFrameworkCore;
using CoeurApi.Modules.Shopping.Application.Abstractions;
using CoeurApi.Modules.Shopping.Domain;

namespace CoeurApi.Modules.Shopping.Infrastructure.Repository;

public class ShoppingListRepository(DbContext context) : IShoppingListRepository
{
    public async Task<(List<ShoppingList> Items, int TotalCount)> GetAllByOwnerAsync(Guid ownerId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = context.Set<ShoppingList>()
            .Where(l => l.OwnerId == ownerId)
            .OrderByDescending(l => l.UpdatedAt);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<ShoppingList?> GetByIdWithItemsAsync(Guid id, CancellationToken cancellationToken = default)
        => await context.Set<ShoppingList>()
            .Include(l => l.Items)
                .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(l => l.Id == id, cancellationToken);

    public async Task<ListItem?> GetItemAsync(Guid listId, Guid itemId, CancellationToken cancellationToken = default)
        => await context.Set<ListItem>()
            .FirstOrDefaultAsync(i => i.ShoppingListId == listId && i.Id == itemId, cancellationToken);

    public async Task AddAsync(ShoppingList list, CancellationToken cancellationToken = default)
        => await context.Set<ShoppingList>().AddAsync(list, cancellationToken);

    public async Task AddItemAsync(ListItem item, CancellationToken cancellationToken = default)
        => await context.Set<ListItem>().AddAsync(item, cancellationToken);

    public void Delete(ShoppingList list)
        => context.Set<ShoppingList>().Remove(list);

    public void DeleteItem(ListItem item)
        => context.Set<ListItem>().Remove(item);
}
