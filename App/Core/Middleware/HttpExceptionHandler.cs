using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using CoeurApi.App.Shared.Exceptions;

namespace CoeurApi.App.Core.Middleware;

// Traduz HttpException pra Problem Details (RFC 9457) — o formato padrão do ASP.NET Core
// pra respostas de erro. Qualquer exceção que não seja HttpException retorna false: quem
// trata é o handler default do framework (registrado por UseExceptionHandler()), que já
// loga e gera um Problem 500 — a extension `toast` é adicionada nos dois casos pelo
// CustomizeProblemDetails global (ServiceCollectionExtensions.AddCore()).
public class HttpExceptionHandler(IProblemDetailsService problemDetailsService) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is not HttpException httpException)
            return false;

        httpContext.Response.StatusCode = httpException.StatusCode;

        ProblemDetails problemDetails = httpException.Errors is not null
            ? new ValidationProblemDetails(new Dictionary<string, string[]>(httpException.Errors)) { Status = httpException.StatusCode, Detail = httpException.Message }
            : new ProblemDetails { Status = httpException.StatusCode, Detail = httpException.Message };

        await problemDetailsService.WriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = problemDetails,
            Exception = exception
        });

        return true;
    }
}
