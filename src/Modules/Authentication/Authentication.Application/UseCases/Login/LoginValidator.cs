using FluentValidation;

namespace CoeurApi.Modules.Authentication.Application.UseCases;

public class LoginValidator : AbstractValidator<LoginRequest>
{
    public LoginValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email é obrigatório.")
            .MaximumLength(150).WithMessage("Email deve ter no máximo 150 caracteres.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Senha é obrigatória.");
    }
}
