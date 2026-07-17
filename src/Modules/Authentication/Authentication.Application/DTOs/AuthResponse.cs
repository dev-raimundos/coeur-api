using CoeurApi.Modules.Users.Application.DTOs;

namespace CoeurApi.Modules.Authentication.Application.DTOs;

public record AuthResponse(
    UserResponse User
);
