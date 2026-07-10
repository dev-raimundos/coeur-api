using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CoeurApi.App.Modules.Shopping.DTOs;
using CoeurApi.App.Modules.Shopping.Services;

namespace CoeurApi.App.Modules.Shopping.Controllers;

[ApiController]
[Route("api/products")]
public class ProductsController(ProductsService service) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? category)
    {
        var products = await service.GetAllAsync(category);
        return Ok(products);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var product = await service.GetByIdAsync(id);
        return Ok(product);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] CreateProductDto dto)
    {
        var product = await service.CreateAsync(dto);
        return Created($"api/products/{product.Id}", product);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateProductDto dto)
    {
        var product = await service.UpdateAsync(id, dto);
        return Ok(product);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await service.DeleteAsync(id);
        return NoContent();
    }
}
