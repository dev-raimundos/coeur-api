using FluentValidation;

namespace CoeurApi.Modules.Users.Application.UseCases;

public class UpdateUserValidator : AbstractValidator<UpdateUserRequest>
{
    public UpdateUserValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Nome é obrigatório.")
            .MinimumLength(2).WithMessage("Nome deve ter no mínimo 2 caracteres.")
            .MaximumLength(100).WithMessage("Nome deve ter no máximo 100 caracteres.");
    }
}
