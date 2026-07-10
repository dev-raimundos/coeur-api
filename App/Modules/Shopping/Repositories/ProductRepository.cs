using Microsoft.EntityFrameworkCore;
using CoeurApi.App.Core.Database;
using CoeurApi.App.Modules.Shopping.Models;

namespace CoeurApi.App.Modules.Shopping.Repositories;

public class ProductRepository(AppDbContext context) : IProductRepository
{
    public async Task<List<Product>> GetAllAsync(string? category)
    {
        var query = context.Products.AsQueryable();

        if (!string.IsNullOrWhiteSpace(category))
            query = query.Where(p => p.Category == category);

        return await query.OrderBy(p => p.Category).ThenBy(p => p.Name).ToListAsync();
    }

    public async Task<Product?> GetByIdAsync(Guid id)
        => await context.Products.FindAsync(id);

    public async Task AddAsync(Product product)
        => await context.Products.AddAsync(product);

    public void Delete(Product product)
        => context.Products.Remove(product);
}
