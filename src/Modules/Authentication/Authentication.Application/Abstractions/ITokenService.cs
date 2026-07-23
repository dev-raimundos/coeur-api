using CoeurApi.Modules.Users.Domain.Model;

namespace CoeurApi.Modules.Authentication.Application.Abstractions;

public interface ITokenService
{
    string Generate(User user);
}
