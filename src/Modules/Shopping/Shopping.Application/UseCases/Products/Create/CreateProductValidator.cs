using FluentValidation;

namespace CoeurApi.Modules.Shopping.Application.UseCases.Products.Create;

public class CreateProductValidator : AbstractValidator<CreateProductRequest>
{
    public CreateProductValidator()
    {
        this.ApplyProductFieldsRules();
    }
}
