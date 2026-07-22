namespace CoeurApi.Modules.Shopping.Application.UseCases.ShoppingLists;

public record AddShoppingListItemRequest(
    string Name,
    int Quantity = 1,
    string? Unit = null,
    Guid? ProductId = null
);
