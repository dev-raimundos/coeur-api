using FluentValidation;
using CoeurApi.Modules.Shopping.Application.DTOs;

namespace CoeurApi.Modules.Shopping.Application.Validators;

public class CreateProductDtoValidator : AbstractValidator<CreateProductDto>
{
    public CreateProductDtoValidator()
    {
        this.ApplyProductFieldsRules();
    }
}
