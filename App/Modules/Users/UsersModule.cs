using NeonVertexApi.App.Modules.Users.Repositories;
using NeonVertexApi.App.Modules.Users.Services;
using NeonVertexApi.App.Shared.Interfaces;

namespace NeonVertexApi.App.Modules.Users;

public static class UsersModule
{
    public static IServiceCollection AddUsersModule(this IServiceCollection services)
    {
        services.AddScoped<IUsersRepository, UsersRepository>();
        services.AddScoped<UsersService>();

        return services;
    }
}