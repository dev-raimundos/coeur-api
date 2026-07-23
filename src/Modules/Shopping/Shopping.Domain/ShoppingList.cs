using CoeurApi.Modules.Users.Domain.Model;

namespace CoeurApi.Modules.Shopping.Domain;

public class ShoppingList
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = null!;
    public Guid OwnerId { get; private set; }
    public User Owner { get; private set; } = null!;
    public ICollection<ListItem> Items { get; private set; } = [];
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    public static ShoppingList Create(string name, Guid ownerId) => new()
    {
        Id = Guid.NewGuid(),
        Name = name,
        OwnerId = ownerId,
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow
    };

    public void Rename(string name)
    {
        Name = name;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Touch()
    {
        UpdatedAt = DateTime.UtcNow;
    }
}
