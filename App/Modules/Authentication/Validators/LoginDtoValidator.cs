using FluentValidation;
using CoeurApi.App.Modules.Authentication.DTOs;

namespace CoeurApi.App.Modules.Authentication.Validators;

public class LoginDtoValidator : AbstractValidator<LoginDto>
{
    public LoginDtoValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email é obrigatório.")
            .MaximumLength(150).WithMessage("Email deve ter no máximo 150 caracteres.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Senha é obrigatória.");
    }
}
