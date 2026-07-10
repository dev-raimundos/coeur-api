namespace CoeurApi.App.Modules.Shopping.DTOs;

public record UpdateProductDto(string Name, string Category, string? ImageUrl) : IProductFields;
