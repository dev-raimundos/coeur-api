namespace CoeurApi.Modules.Authentication.Application.UseCases.Login;

public record LoginRequest(
    string Email,
    string Password
);
