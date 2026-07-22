namespace CoeurApi.Modules.Shopping.Application.UseCases.ShoppingLists;

public record UpdateShoppingListItemRequest(
    string? Name = null,
    int? Quantity = null,
    string? Unit = null,
    bool? IsChecked = null
);
