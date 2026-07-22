namespace CoeurApi.Modules.Shopping.Application.UseCases.Products;

public interface IProductFields
{
    string Name { get; }
    string Category { get; }
    string? ImageUrl { get; }
}
