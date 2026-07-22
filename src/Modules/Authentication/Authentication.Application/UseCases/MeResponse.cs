namespace CoeurApi.Modules.Authentication.Application.UseCases;

public record MeResponse(
    Guid Id,
    string Name,
    string Email
);
