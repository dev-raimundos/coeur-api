namespace CoeurApi.Modules.Authentication.Application.UseCases;

public record LoginRequest(
    string Email,
    string Password
);
