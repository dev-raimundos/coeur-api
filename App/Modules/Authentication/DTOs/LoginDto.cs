namespace CoeurApi.App.Modules.Authentication.DTOs;

public record LoginDto(
    string Email,
    string Password
);