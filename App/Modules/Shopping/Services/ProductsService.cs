using CoeurApi.App.Core.Database;
using CoeurApi.App.Modules.Shopping.DTOs;
using CoeurApi.App.Modules.Shopping.Models;
using CoeurApi.App.Modules.Shopping.Repositories;
using CoeurApi.App.Shared.Exceptions;

namespace CoeurApi.App.Modules.Shopping.Services;

public class ProductsService(IProductRepository repository, AppDbContext context)
{
    private const string ErrNotFound = "Produto não encontrado.";

    public async Task<List<ProductResponse>> GetAllAsync(string? category)
    {
        var products = await repository.GetAllAsync(category);
        return products.Select(ProductResponse.FromEntity).ToList();
    }

    public async Task<ProductResponse> GetByIdAsync(Guid id)
    {
        var product = await repository.GetByIdAsync(id)
            ?? throw AppException.NotFound(ErrNotFound);

        return ProductResponse.FromEntity(product);
    }

    public async Task<ProductResponse> CreateAsync(CreateProductDto dto)
    {
        var product = Product.Create(dto.Name, dto.Category, dto.ImageUrl);
        await repository.AddAsync(product);
        await context.SaveChangesAsync();

        return ProductResponse.FromEntity(product);
    }

    public async Task<ProductResponse> UpdateAsync(Guid id, UpdateProductDto dto)
    {
        var product = await repository.GetByIdAsync(id)
            ?? throw AppException.NotFound(ErrNotFound);

        product.Update(dto.Name, dto.Category, dto.ImageUrl);
        await context.SaveChangesAsync();

        return ProductResponse.FromEntity(product);
    }

    public async Task DeleteAsync(Guid id)
    {
        var product = await repository.GetByIdAsync(id)
            ?? throw AppException.NotFound(ErrNotFound);

        repository.Delete(product);
        await context.SaveChangesAsync();
    }
}
