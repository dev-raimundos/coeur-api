using FluentValidation;
using CoeurApi.App.Modules.Shopping.DTOs;

namespace CoeurApi.App.Modules.Shopping.Validators;

public class CreateProductDtoValidator : AbstractValidator<CreateProductDto>
{
    public CreateProductDtoValidator()
    {
        this.ApplyProductFieldsRules();
    }
}
