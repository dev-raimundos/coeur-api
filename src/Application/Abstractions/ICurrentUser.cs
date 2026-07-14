using CoeurApi.SharedKernel.Enums;

namespace CoeurApi.Application.Abstractions;

public interface ICurrentUser
{
    Guid Id { get; }
    string Email { get; }
    string Name { get; }
    UserRole Role { get; }
    bool IsAdmin { get; }
    bool IsAuthenticated { get; }
}
