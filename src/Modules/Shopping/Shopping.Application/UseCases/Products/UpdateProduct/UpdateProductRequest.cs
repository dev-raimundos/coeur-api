namespace CoeurApi.Modules.Shopping.Application.UseCases.Products;

public record UpdateProductRequest(string Name, string Category, string? ImageUrl) : IProductFields;
