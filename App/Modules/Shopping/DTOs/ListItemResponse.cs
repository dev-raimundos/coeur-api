using CoeurApi.App.Modules.Shopping.Models;

namespace CoeurApi.App.Modules.Shopping.DTOs;

public record ListItemResponse(
    Guid Id,
    Guid ShoppingListId,
    Guid? ProductId,
    string Name,
    int Quantity,
    string? Unit,
    bool IsChecked,
    DateTime CreatedAt
)
{
    public static ListItemResponse FromEntity(ListItem item) => new(
        item.Id,
        item.ShoppingListId,
        item.ProductId,
        item.Name,
        item.Quantity,
        item.Unit,
        item.IsChecked,
        item.CreatedAt
    );
}
