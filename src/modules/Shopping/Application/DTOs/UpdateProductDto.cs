namespace CoeurApi.Modules.Shopping.Application.DTOs;

public record UpdateProductDto(string Name, string Category, string? ImageUrl) : IProductFields;
