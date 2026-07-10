using CoeurApi.App.Modules.Users.DTOs;

namespace CoeurApi.App.Modules.Authentication.DTOs;

public record AuthResponse(
    UserResponse User
);
