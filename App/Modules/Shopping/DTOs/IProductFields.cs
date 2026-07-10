namespace CoeurApi.App.Modules.Shopping.DTOs;

public interface IProductFields
{
    string Name { get; }
    string Category { get; }
    string? ImageUrl { get; }
}
