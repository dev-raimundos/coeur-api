using CoeurApi.Modules.Shopping.Application.Abstractions;
using CoeurApi.Modules.Shopping.Application.DTOs;
using CoeurApi.SharedKernel.Common;

namespace CoeurApi.Modules.Shopping.Application.Services.Products;

public class GetAllProductsService(IProductRepository repository)
{
    public async Task<PagedResult<ProductResponse>> ExecuteAsync(string? category, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var (products, totalCount) = await repository.GetAllAsync(category, page, pageSize, cancellationToken);
        return new PagedResult<ProductResponse>(products.Select(ProductResponse.FromEntity).ToList(), page, pageSize, totalCount);
    }
}
