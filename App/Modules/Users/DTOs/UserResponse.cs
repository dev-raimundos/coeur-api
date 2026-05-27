using NeonVertexApi.App.Modules.Users.Models;

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
)
{
    public static UserResponse FromEntity(User user) => new(
        user.Id,
        user.Name,
        user.Email,
        user.IsActive,
        user.IsEmailVerified,
        user.CreatedAt,
        user.UpdatedAt,
        user.LastLoginAt
    );
};