namespace CoeurApi.Modules.Shopping.Application.UseCases.ShoppingLists.UpdateItem;

public record UpdateShoppingListItemRequest(
    string? Name = null,
    int? Quantity = null,
    string? Unit = null,
    bool? IsChecked = null
);
