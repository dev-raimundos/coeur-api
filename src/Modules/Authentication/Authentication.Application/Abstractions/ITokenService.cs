using CoeurApi.Modules.Users.Domain;

namespace CoeurApi.Modules.Authentication.Application.Abstractions;

public interface ITokenService
{
    string Generate(User user);
}
