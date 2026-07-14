using Microsoft.EntityFrameworkCore;
using CoeurApi.Modules.Shopping.Domain;
using CoeurApi.Modules.Users.Domain;
using CoeurApi.SharedKernel.Abstractions;

namespace CoeurApi.Infrastructure.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options), IUnitOfWork
{
    public DbSet<User> Users => Set<User>();
    public DbSet<ShoppingList> ShoppingLists => Set<ShoppingList>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ListItem> ListItems => Set<ListItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(User).Assembly);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(Product).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
