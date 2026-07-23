using Microsoft.EntityFrameworkCore;
using CoeurApi.Modules.Shopping.Application.Abstractions;
using CoeurApi.Modules.Shopping.Domain;

namespace CoeurApi.Modules.Shopping.Infrastructure.Repository;

public class ProductRepository(DbContext context) : IProductRepository
{
    public async Task<(List<Product> Items, int TotalCount)> GetAllAsync(string? category, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = context.Set<Product>().AsQueryable();

        if (!string.IsNullOrWhiteSpace(category))
            query = query.Where(p => p.Category == category);

        query = query.OrderBy(p => p.Category).ThenBy(p => p.Name);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await context.Set<Product>().FindAsync([id], cancellationToken);

    public async Task AddAsync(Product product, CancellationToken cancellationToken = default)
        => await context.Set<Product>().AddAsync(product, cancellationToken);

    public void Delete(Product product)
        => context.Set<Product>().Remove(product);
}
