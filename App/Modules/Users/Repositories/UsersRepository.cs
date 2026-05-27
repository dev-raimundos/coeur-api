using Microsoft.EntityFrameworkCore;
using NeonVertexApi.App.Core.Database;
using NeonVertexApi.App.Modules.Users.Models;
using NeonVertexApi.App.Shared.Interfaces;

namespace NeonVertexApi.App.Modules.Users.Repositories;

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

    public async Task UpdateAsync(User user)
        => context.Users.Update(user);

    public async Task DeleteAsync(User user)
        => context.Users.Remove(user);
}