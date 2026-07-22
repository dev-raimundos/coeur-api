using FluentValidation;
using CoeurApi.Modules.Users.Application.Abstractions;
using CoeurApi.Modules.Users.Application.UseCases;

namespace CoeurApi.Modules.Users.Infrastructure;

public static class UsersModule
{
    public static IServiceCollection AddUsersModule(this IServiceCollection services)
    {
        services.AddScoped<IUsersRepository, UsersRepository>();
        services.AddScoped<CreateUserUseCase>();
        services.AddScoped<GetUserByIdUseCase>();
        services.AddScoped<UpdateUserUseCase>();
        services.AddScoped<DeleteUserUseCase>();

        services.AddValidatorsFromAssemblyContaining<CreateUserValidator>();

        return services;
    }
}
