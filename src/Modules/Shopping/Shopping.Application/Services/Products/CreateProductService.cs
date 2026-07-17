using CoeurApi.Modules.Shopping.Application.Abstractions;
using CoeurApi.Modules.Shopping.Application.DTOs;
using CoeurApi.Modules.Shopping.Domain;
using CoeurApi.SharedKernel.Abstractions;

namespace CoeurApi.Modules.Shopping.Application.Services.Products;

public class CreateProductService(IProductRepository repository, IUnitOfWork unitOfWork)
{
    public async Task<ProductResponse> ExecuteAsync(CreateProductDto dto, CancellationToken cancellationToken = default)
    {
        var product = Product.Create(dto.Name, dto.Category, dto.ImageUrl);
        await repository.AddAsync(product, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return ProductResponse.FromEntity(product);
    }
}
