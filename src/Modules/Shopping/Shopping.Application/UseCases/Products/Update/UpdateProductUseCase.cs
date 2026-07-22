using CoeurApi.Modules.Shopping.Application.Abstractions;
using CoeurApi.Modules.Shopping.Application.UseCases.Products;
using CoeurApi.SharedKernel.Abstractions;
using CoeurApi.SharedKernel.Exceptions;

namespace CoeurApi.Modules.Shopping.Application.UseCases.Products.Update;

public class UpdateProductUseCase(IProductRepository repository, IUnitOfWork unitOfWork)
{
    private const string ErrNotFound = "Produto não encontrado.";

    public async Task<ProductResponse> ExecuteAsync(Guid id, UpdateProductRequest request, CancellationToken cancellationToken = default)
    {
        var product = await repository.GetByIdAsync(id, cancellationToken)
            ?? throw HttpException.NotFound(ErrNotFound);

        product.Update(request.Name, request.Category, request.ImageUrl);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return ProductResponse.FromEntity(product);
    }
}
