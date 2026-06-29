using FluentValidation;
using Microsoft.AspNetCore.Mvc.Filters;
using NeonVertexApi.App.Shared.Exceptions;

namespace NeonVertexApi.App.Core.Filters;

public class FluentValidationFilter(IServiceProvider serviceProvider) : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        foreach (var argument in context.ActionArguments.Values)
        {
            if (argument is null) continue;

            var validatorType = typeof(IValidator<>).MakeGenericType(argument.GetType());
            if (serviceProvider.GetService(validatorType) is not IValidator validator) continue;

            var validationContext = new ValidationContext<object>(argument);
            var result = await validator.ValidateAsync(validationContext);

            if (!result.IsValid)
            {
                var errors = result.Errors
                    .GroupBy(e => char.ToLower(e.PropertyName[0]) + e.PropertyName[1..])
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(e => e.ErrorMessage).ToArray()
                    ) as IReadOnlyDictionary<string, string[]>;

                throw AppException.BadRequest("Dados inválidos.", errors);
            }
        }

        await next();
    }
}
