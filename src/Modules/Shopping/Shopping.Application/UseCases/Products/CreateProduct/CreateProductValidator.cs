using FluentValidation;

namespace CoeurApi.Modules.Shopping.Application.UseCases.Products;

public class CreateProductValidator : AbstractValidator<CreateProductRequest>
{
    public CreateProductValidator()
    {
        this.ApplyProductFieldsRules();
    }
}
