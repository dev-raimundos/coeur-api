using Microsoft.EntityFrameworkCore;
using CoeurApi.Application.Abstractions;
using CoeurApi.Infrastructure.Authentication;
using CoeurApi.Infrastructure.Persistence;
using CoeurApi.SharedKernel.Abstractions;

namespace CoeurApi.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options => options.UseNpgsql(configuration.GetConnectionString("Default")));
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<AppDbContext>());
        services.AddScoped<DbContext>(sp => sp.GetRequiredService<AppDbContext>());

        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUser, CurrentUserService>();

        return services;
    }
}
