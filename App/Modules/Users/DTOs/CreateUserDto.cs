namespace CoeurApi.App.Modules.Users.DTOs;

public record CreateUserDto(
    string Name,
    string Email,
    string Password
);