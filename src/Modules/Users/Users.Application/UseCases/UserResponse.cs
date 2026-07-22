using CoeurApi.Modules.Users.Domain;

namespace CoeurApi.Modules.Users.Application.UseCases;

public record UserResponse(
    Guid Id,
    string Name,
    string Email,
    string Role,
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
        user.Role.ToString(),
        user.IsActive,
        user.IsEmailVerified,
        user.CreatedAt,
        user.UpdatedAt,
        user.LastLoginAt
    );
};
