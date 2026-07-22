using CoeurApi.Modules.Shopping.Application.UseCases.Products;

namespace CoeurApi.Modules.Shopping.Application.UseCases.Products.Create;

public record CreateProductRequest(string Name, string Category, string? ImageUrl = null) : IProductFields;
