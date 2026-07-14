using FluentValidation;
using CoeurApi.Modules.Shopping.Application.DTOs;

namespace CoeurApi.Modules.Shopping.Application.Validators;

public class UpdateProductDtoValidator : AbstractValidator<UpdateProductDto>
{
    public UpdateProductDtoValidator()
    {
        this.ApplyProductFieldsRules();
    }
}
