using Microsoft.EntityFrameworkCore;
using CoeurApi.App.Modules.Shopping.Models;
using CoeurApi.App.Modules.Users.Models;

namespace CoeurApi.App.Core.Database;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<ShoppingList> ShoppingLists => Set<ShoppingList>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ListItem> ListItems => Set<ListItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}