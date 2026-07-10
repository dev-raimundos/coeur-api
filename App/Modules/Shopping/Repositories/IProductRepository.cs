using CoeurApi.App.Modules.Shopping.Models;

namespace CoeurApi.App.Modules.Shopping.Repositories;

public interface IProductRepository
{
    Task<List<Product>> GetAllAsync(string? category);
    Task<Product?> GetByIdAsync(Guid id);
    Task AddAsync(Product product);
    void Delete(Product product);
}
