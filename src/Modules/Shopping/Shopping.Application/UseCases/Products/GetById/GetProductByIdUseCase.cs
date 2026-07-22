using CoeurApi.Modules.Shopping.Application.Abstractions;
using CoeurApi.Modules.Shopping.Application.UseCases.Products;
using CoeurApi.SharedKernel.Exceptions;

namespace CoeurApi.Modules.Shopping.Application.UseCases.Products.GetById;

public class GetProductByIdUseCase(IProductRepository repository)
{
    private const string ErrNotFound = "Produto não encontrado.";

    public async Task<ProductResponse> ExecuteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var product = await repository.GetByIdAsync(id, cancellationToken)
            ?? throw HttpException.NotFound(ErrNotFound);

        return ProductResponse.FromEntity(product);
    }
}
