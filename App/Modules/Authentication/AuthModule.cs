using NeonVertexApi.App.Modules.Authentication.Services;

namespace NeonVertexApi.App.Modules.Authentication;

public static class AuthModule
{
    public static IServiceCollection AddAuthModule(this IServiceCollection services)
    {
        services.AddScoped<AuthService>();
        return services;
    }
}
