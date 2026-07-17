namespace CoeurApi.Modules.Users.Application.DTOs;

public record CreateUserDto(
    string Name,
    string Email,
    string Password
);
