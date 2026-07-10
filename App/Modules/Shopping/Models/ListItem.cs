namespace CoeurApi.App.Modules.Shopping.Models;

public class ListItem
{
    public Guid Id { get; private set; }
    public Guid ShoppingListId { get; private set; }
    public ShoppingList ShoppingList { get; private set; } = null!;
    public Guid? ProductId { get; private set; }
    public Product? Product { get; private set; }
    public string Name { get; private set; } = null!;
    public int Quantity { get; private set; }
    public string? Unit { get; private set; }
    public bool IsChecked { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public static ListItem Create(Guid shoppingListId, string name, int quantity = 1, string? unit = null, Guid? productId = null) => new()
    {
        Id = Guid.NewGuid(),
        ShoppingListId = shoppingListId,
        ProductId = productId,
        Name = name,
        Quantity = quantity,
        Unit = unit,
        IsChecked = false,
        CreatedAt = DateTime.UtcNow
    };

    public void Check() => IsChecked = true;
    public void Uncheck() => IsChecked = false;

    public void UpdateDetails(string name, int quantity, string? unit)
    {
        Name = name;
        Quantity = quantity;
        Unit = unit;
    }
}
