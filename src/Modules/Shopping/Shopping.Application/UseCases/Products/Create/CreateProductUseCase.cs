using CoeurApi.Modules.Shopping.Application.Abstractions;
using CoeurApi.Modules.Shopping.Application.UseCases.Products;
using CoeurApi.Modules.Shopping.Domain;
using CoeurApi.SharedKernel.Abstractions;

namespace CoeurApi.Modules.Shopping.Application.UseCases.Products.Create;

public class CreateProductUseCase(IProductRepository repository, IUnitOfWork unitOfWork)
{
    public async Task<ProductResponse> ExecuteAsync(CreateProductRequest request, CancellationToken cancellationToken = default)
    {
        var product = Product.Create(request.Name, request.Category, request.ImageUrl);
        await repository.AddAsync(product, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return ProductResponse.FromEntity(product);
    }
}
