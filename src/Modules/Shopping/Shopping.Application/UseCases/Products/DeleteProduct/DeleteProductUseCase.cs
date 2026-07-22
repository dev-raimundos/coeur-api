using CoeurApi.Modules.Shopping.Application.Abstractions;
using CoeurApi.SharedKernel.Abstractions;
using CoeurApi.SharedKernel.Exceptions;

namespace CoeurApi.Modules.Shopping.Application.UseCases.Products;

public class DeleteProductUseCase(IProductRepository repository, IUnitOfWork unitOfWork)
{
    private const string ErrNotFound = "Produto não encontrado.";

    public async Task ExecuteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var product = await repository.GetByIdAsync(id, cancellationToken)
            ?? throw HttpException.NotFound(ErrNotFound);

        repository.Delete(product);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
