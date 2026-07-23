using Microsoft.EntityFrameworkCore;
using CoeurApi.Modules.Users.Application.Abstractions;
using CoeurApi.Modules.Users.Domain.Model;

namespace CoeurApi.Modules.Users.Infrastructure.Repository;

public class UsersRepository(DbContext context) : IUsersRepository
{
    public async Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await context.Set<User>().FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
        => await context.Set<User>().FirstOrDefaultAsync(u => u.Email == email.ToLowerInvariant(), cancellationToken);

    public async Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default)
        => await context.Set<User>().AnyAsync(u => u.Email == email.ToLowerInvariant(), cancellationToken);

    public async Task AddAsync(User user, CancellationToken cancellationToken = default)
        => await context.Set<User>().AddAsync(user, cancellationToken);

    public void Delete(User user)
        => context.Set<User>().Remove(user);
}
