using NeonVertexApi.App.Modules.Users.Models;

namespace NeonVertexApi.App.Shared.Interfaces;

public interface ICurrentUser
{
    Guid Id { get; }
    string Email { get; }
    string Name { get; }
    UserRole Role { get; }
    bool IsAdmin { get; }
    bool IsAuthenticated { get; }
}
