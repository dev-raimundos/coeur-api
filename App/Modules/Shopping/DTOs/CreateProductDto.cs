namespace CoeurApi.App.Modules.Shopping.DTOs;

public record CreateProductDto(string Name, string Category, string? ImageUrl = null) : IProductFields;
