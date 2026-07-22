namespace CoeurApi.Modules.Users.Application.UseCases;

public record CreateUserRequest(
    string Name,
    string Email,
    string Password
);
