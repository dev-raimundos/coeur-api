namespace NeonVertexApi.App.Modules.Users.DTOs;

public record UserResponse(
    Guid Id,
    string Name,
    string Email,
    bool IsActive,
    bool IsEmailVerified,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    DateTime? LastLoginAt
);