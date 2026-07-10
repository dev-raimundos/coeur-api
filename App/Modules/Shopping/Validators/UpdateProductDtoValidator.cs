using FluentValidation;
using CoeurApi.App.Modules.Shopping.DTOs;

namespace CoeurApi.App.Modules.Shopping.Validators;

public class UpdateProductDtoValidator : AbstractValidator<UpdateProductDto>
{
    public UpdateProductDtoValidator()
    {
        this.ApplyProductFieldsRules();
    }
}
