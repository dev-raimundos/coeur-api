using CoeurApi.Modules.Shopping.Application.UseCases.Products;

namespace CoeurApi.Modules.Shopping.Application.UseCases.Products.Update;

public record UpdateProductRequest(string Name, string Category, string? ImageUrl) : IProductFields;
