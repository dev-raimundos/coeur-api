using FluentValidation;
using NeonVertexApi.App.Modules.Shopping.DTOs;

namespace NeonVertexApi.App.Modules.Shopping.Validators;

public class CreateProductDtoValidator : AbstractValidator<CreateProductDto>
{
    public CreateProductDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Nome é obrigatório.")
            .MaximumLength(150).WithMessage("Nome deve ter no máximo 150 caracteres.");

        RuleFor(x => x.Category)
            .NotEmpty().WithMessage("Categoria é obrigatória.")
            .MaximumLength(100).WithMessage("Categoria deve ter no máximo 100 caracteres.");

        RuleFor(x => x.ImageUrl)
            .Must(url => Uri.TryCreate(url, UriKind.Absolute, out _))
            .When(x => !string.IsNullOrEmpty(x.ImageUrl))
            .WithMessage("URL da imagem inválida.")
            .MaximumLength(500).When(x => x.ImageUrl != null)
            .WithMessage("URL da imagem deve ter no máximo 500 caracteres.");
    }
}
