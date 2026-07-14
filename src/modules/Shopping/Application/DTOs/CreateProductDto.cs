namespace CoeurApi.Modules.Shopping.Application.DTOs;

public record CreateProductDto(string Name, string Category, string? ImageUrl = null) : IProductFields;
