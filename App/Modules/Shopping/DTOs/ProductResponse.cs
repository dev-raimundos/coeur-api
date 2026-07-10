using CoeurApi.App.Modules.Shopping.Models;

namespace CoeurApi.App.Modules.Shopping.DTOs;

public record ProductResponse(
    Guid Id,
    string Name,
    string Category,
    string? ImageUrl,
    DateTime CreatedAt
)
{
    public static ProductResponse FromEntity(Product product) => new(
        product.Id,
        product.Name,
        product.Category,
        product.ImageUrl,
        product.CreatedAt
    );
}
