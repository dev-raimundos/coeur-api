namespace CoeurApi.Modules.Authentication.Application.DTOs;

public record MeResponse(
    Guid Id,
    string Name,
    string Email
);
