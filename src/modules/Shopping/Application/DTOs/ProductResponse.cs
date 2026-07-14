using CoeurApi.Modules.Shopping.Domain;

namespace CoeurApi.Modules.Shopping.Application.DTOs;

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
