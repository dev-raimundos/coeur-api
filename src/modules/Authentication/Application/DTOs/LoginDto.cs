namespace CoeurApi.Modules.Authentication.Application.DTOs;

public record LoginDto(
    string Email,
    string Password
);
