using Microsoft.EntityFrameworkCore;
using CoeurApi.App.Core.Database;
using CoeurApi.App.Modules.Users.Models;
using CoeurApi.App.Shared.Interfaces;

namespace CoeurApi.App.Modules.Users.Repositories;

public class UsersRepository(AppDbContext context) : IUsersRepository
{
    public async Task<User?> GetByIdAsync(Guid id)
        => await context.Users.FirstOrDefaultAsync(u => u.Id == id);

    public async Task<User?> GetByEmailAsync(string email)
        => await context.Users.FirstOrDefaultAsync(u => u.Email == email.ToLowerInvariant());

    public async Task<bool> ExistsByEmailAsync(string email)
        => await context.Users.AnyAsync(u => u.Email == email.ToLowerInvariant());

    public async Task AddAsync(User user)
        => await context.Users.AddAsync(user);

    public void Delete(User user)
        => context.Users.Remove(user);
}