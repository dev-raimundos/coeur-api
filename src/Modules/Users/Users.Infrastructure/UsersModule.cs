using FluentValidation;
using CoeurApi.Modules.Users.Application.Abstractions;
using CoeurApi.Modules.Users.Application.Services;
using CoeurApi.Modules.Users.Application.Validators;
using CoeurApi.Modules.Users.Infrastructure;

namespace CoeurApi.Modules.Users;

public static class UsersModule
{
    public static IServiceCollection AddUsersModule(this IServiceCollection services)
    {
        services.AddScoped<IUsersRepository, UsersRepository>();
        services.AddScoped<CreateUserService>();
        services.AddScoped<GetUserByIdService>();
        services.AddScoped<UpdateUserService>();
        services.AddScoped<DeleteUserService>();

        services.AddValidatorsFromAssemblyContaining<CreateUserDtoValidator>();

        return services;
    }
}
