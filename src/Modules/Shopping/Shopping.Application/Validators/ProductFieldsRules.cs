using FluentValidation;
using CoeurApi.Modules.Shopping.Application.DTOs;

namespace CoeurApi.Modules.Shopping.Application.Validators;

public static class ProductFieldsRules
{
    public static void ApplyProductFieldsRules<T>(this AbstractValidator<T> validator) where T : IProductFields
    {
        validator.RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Nome é obrigatório.")
            .MaximumLength(150).WithMessage("Nome deve ter no máximo 150 caracteres.");

        validator.RuleFor(x => x.Category)
            .NotEmpty().WithMessage("Categoria é obrigatória.")
            .MaximumLength(100).WithMessage("Categoria deve ter no máximo 100 caracteres.");

        validator.RuleFor(x => x.ImageUrl)
            .Must(url => Uri.TryCreate(url, UriKind.Absolute, out _))
            .When(x => !string.IsNullOrEmpty(x.ImageUrl))
            .WithMessage("URL da imagem inválida.")
            .MaximumLength(500).When(x => x.ImageUrl != null)
            .WithMessage("URL da imagem deve ter no máximo 500 caracteres.");
    }
}
