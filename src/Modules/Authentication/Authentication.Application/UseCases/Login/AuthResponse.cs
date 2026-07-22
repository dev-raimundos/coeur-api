using CoeurApi.Modules.Users.Application.UseCases;

namespace CoeurApi.Modules.Authentication.Application.UseCases;

public record AuthResponse(
    UserResponse User
);
