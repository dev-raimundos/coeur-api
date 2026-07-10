namespace CoeurApi.App.Modules.Shopping.Models;

public class Product
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = null!;
    public string Category { get; private set; } = null!;
    public string? ImageUrl { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public static Product Create(string name, string category, string? imageUrl = null) => new()
    {
        Id = Guid.NewGuid(),
        Name = name,
        Category = category,
        ImageUrl = imageUrl,
        CreatedAt = DateTime.UtcNow
    };

    public void Update(string name, string category, string? imageUrl)
    {
        Name = name;
        Category = category;
        ImageUrl = imageUrl;
    }
}
