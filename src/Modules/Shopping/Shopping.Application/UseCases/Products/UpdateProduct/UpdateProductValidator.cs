using FluentValidation;

namespace CoeurApi.Modules.Shopping.Application.UseCases.Products;

public class UpdateProductValidator : AbstractValidator<UpdateProductRequest>
{
    public UpdateProductValidator()
    {
        this.ApplyProductFieldsRules();
    }
}
