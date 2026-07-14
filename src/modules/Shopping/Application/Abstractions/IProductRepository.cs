using CoeurApi.Modules.Shopping.Domain;

namespace CoeurApi.Modules.Shopping.Application.Abstractions;

public interface IProductRepository
{
    Task<(List<Product> Items, int TotalCount)> GetAllAsync(string? category, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(Product product, CancellationToken cancellationToken = default);
    void Delete(Product product);
}
