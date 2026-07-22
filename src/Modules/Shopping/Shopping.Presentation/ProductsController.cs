using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CoeurApi.Modules.Shopping.Application.UseCases.Products;
using CoeurApi.Modules.Shopping.Application.UseCases.Products.Create;
using CoeurApi.Modules.Shopping.Application.UseCases.Products.Delete;
using CoeurApi.Modules.Shopping.Application.UseCases.Products.GetAll;
using CoeurApi.Modules.Shopping.Application.UseCases.Products.GetById;
using CoeurApi.Modules.Shopping.Application.UseCases.Products.Update;
using CoeurApi.SharedKernel.Common;

namespace CoeurApi.Modules.Shopping.Presentation;

[ApiController]
[Route("api/v1/products")]
public class ProductsController(
    GetAllProductsUseCase getAllProducts,
    GetProductByIdUseCase getProductById,
    CreateProductUseCase createProduct,
    UpdateProductUseCase updateProduct,
    DeleteProductUseCase deleteProduct) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<PagedResult<ProductResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<PagedResult<ProductResponse>>> GetAll(
        [FromQuery] string? category,
        [FromQuery] int? page,
        [FromQuery] int? pageSize,
        CancellationToken cancellationToken)
    {
        var (normalizedPage, normalizedPageSize) = Pagination.Normalize(page, pageSize);
        var products = await getAllProducts.ExecuteAsync(category, normalizedPage, normalizedPageSize, cancellationToken);
        return Ok(products);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType<ProductResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var product = await getProductById.ExecuteAsync(id, cancellationToken);
        return Ok(product);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType<ProductResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ProductResponse>> Create([FromBody] CreateProductRequest request, CancellationToken cancellationToken)
    {
        var product = await createProduct.ExecuteAsync(request, cancellationToken);
        return Created($"api/v1/products/{product.Id}", product);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType<ProductResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductResponse>> Update(Guid id, [FromBody] UpdateProductRequest request, CancellationToken cancellationToken)
    {
        var product = await updateProduct.ExecuteAsync(id, request, cancellationToken);
        return Ok(product);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await deleteProduct.ExecuteAsync(id, cancellationToken);
        return NoContent();
    }
}
