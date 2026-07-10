using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CoeurApi.App.Modules.Shopping.DTOs;
using CoeurApi.App.Modules.Shopping.Services.Products;
using CoeurApi.App.Shared.DTOs;

namespace CoeurApi.App.Modules.Shopping.Controllers;

[ApiController]
[Route("api/v1/products")]
public class ProductsController(
    GetAllProductsService getAllProducts,
    GetProductByIdService getProductById,
    CreateProductService createProduct,
    UpdateProductService updateProduct,
    DeleteProductService deleteProduct) : ControllerBase
{
    [HttpGet]
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
    public async Task<ActionResult<ProductResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var product = await getProductById.ExecuteAsync(id, cancellationToken);
        return Ok(product);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ProductResponse>> Create([FromBody] CreateProductDto dto, CancellationToken cancellationToken)
    {
        var product = await createProduct.ExecuteAsync(dto, cancellationToken);
        return Created($"api/v1/products/{product.Id}", product);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ProductResponse>> Update(Guid id, [FromBody] UpdateProductDto dto, CancellationToken cancellationToken)
    {
        var product = await updateProduct.ExecuteAsync(id, dto, cancellationToken);
        return Ok(product);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await deleteProduct.ExecuteAsync(id, cancellationToken);
        return NoContent();
    }
}
