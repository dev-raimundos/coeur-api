namespace CoeurApi.Modules.Users.Application.UseCases.Create;

public record CreateUserRequest(
    string Name,
    string Email,
    string Password
);
