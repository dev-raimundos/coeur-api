using NeonVertexApi.App.Shared.Exceptions;
using System.Text.Json;

namespace NeonVertexApi.App.Core.Middleware;

public class ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (AppException ex)
        {
            await WriteResponseAsync(context, ex.StatusCode, ex.Message, ex.ToastType, ex.Errors);
        }
            catch (Exception ex)
            {
                logger.LogError(ex, "Erro inesperado: {Message}", ex.Message);
                await WriteResponseAsync(context, 500, "Erro interno do servidor.", "error");
            }
    }

    private static async Task WriteResponseAsync(
    HttpContext context,
    int statusCode,
    string message,
    string toastType,
    IReadOnlyDictionary<string, string[]>? errors = null)
    {
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";

        var body = new Dictionary<string, object?>
        {
            ["message"] = message,
            ["toast"] = new { type = toastType, message },
        };

        if (errors is not null)
            body["errors"] = errors;

        var json = JsonSerializer.Serialize(body, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(json);
    }
}