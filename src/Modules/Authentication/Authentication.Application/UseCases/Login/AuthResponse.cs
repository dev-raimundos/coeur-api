using CoeurApi.Modules.Users.Application.UseCases;

namespace CoeurApi.Modules.Authentication.Application.UseCases.Login;

public record AuthResponse(
    UserResponse User
);
