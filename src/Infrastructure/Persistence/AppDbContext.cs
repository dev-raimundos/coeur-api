using Microsoft.EntityFrameworkCore;
using CoeurApi.Modules.Shopping.Domain;
using CoeurApi.Modules.Shopping.Infrastructure.Persistence.Configurations;
using CoeurApi.Modules.Users.Domain;
using CoeurApi.Modules.Users.Infrastructure.Persistence.Configurations;
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
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(UserConfiguration).Assembly);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ProductConfiguration).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}