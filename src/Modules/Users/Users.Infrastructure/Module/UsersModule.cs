using FluentValidation;
using CoeurApi.Modules.Users.Application.Abstractions;
using CoeurApi.Modules.Users.Application.UseCases.Create;
using CoeurApi.Modules.Users.Application.UseCases.Delete;
using CoeurApi.Modules.Users.Application.UseCases.GetById;
using CoeurApi.Modules.Users.Application.UseCases.Update;
using CoeurApi.Modules.Users.Infrastructure.Repository;

namespace CoeurApi.Modules.Users.Infrastructure.Module;

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
