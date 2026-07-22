using FluentValidation;
using CoeurApi.Modules.Shopping.Application.UseCases.Products;

namespace CoeurApi.Modules.Shopping.Application.UseCases.Products.Update;

public class UpdateProductValidator : AbstractValidator<UpdateProductRequest>
{
    public UpdateProductValidator()
    {
        this.ApplyProductFieldsRules();
    }
}
