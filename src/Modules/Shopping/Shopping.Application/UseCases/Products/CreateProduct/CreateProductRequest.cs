namespace CoeurApi.Modules.Shopping.Application.UseCases.Products;

public record CreateProductRequest(string Name, string Category, string? ImageUrl = null) : IProductFields;
