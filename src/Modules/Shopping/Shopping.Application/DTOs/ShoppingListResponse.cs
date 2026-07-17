using CoeurApi.Modules.Shopping.Domain;

namespace CoeurApi.Modules.Shopping.Application.DTOs;

public record ShoppingListResponse(
    Guid Id,
    string Name,
    Guid OwnerId,
    List<ListItemResponse> Items,
    DateTime CreatedAt,
    DateTime UpdatedAt
)
{
    public static ShoppingListResponse FromEntity(ShoppingList list) => new(
        list.Id,
        list.Name,
        list.OwnerId,
        list.Items.Select(ListItemResponse.FromEntity).ToList(),
        list.CreatedAt,
        list.UpdatedAt
    );
}
