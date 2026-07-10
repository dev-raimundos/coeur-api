using System.Net;

namespace CoeurApi.App.Shared.Exceptions;

public class HttpException : Exception
{
    public int StatusCode { get; }
    public IReadOnlyDictionary<string, string[]>? Errors { get; }

    private HttpException(HttpStatusCode statusCode, string message, IReadOnlyDictionary<string, string[]>? errors = null) : base(message)
    {
        StatusCode = (int)statusCode;
        Errors = errors;
    }

    // -------------------------------------------------------------------------
    // 2xx
    // -------------------------------------------------------------------------

    public static HttpException NoContent(string message = "Sem conteúdo.")
        => new(HttpStatusCode.NoContent, message);

    // -------------------------------------------------------------------------
    // 4xx
    // -------------------------------------------------------------------------

    public static HttpException BadRequest(string message = "Requisição inválida.", IReadOnlyDictionary<string, string[]>? errors = null)
        => new(HttpStatusCode.BadRequest, message, errors);

    public static HttpException Unauthorized(string message = "Não autenticado.")
        => new(HttpStatusCode.Unauthorized, message);

    public static HttpException Forbidden(string message = "Acesso negado.")
        => new(HttpStatusCode.Forbidden, message);

    public static HttpException NotFound(string message = "Recurso não encontrado.")
        => new(HttpStatusCode.NotFound, message);

    public static HttpException Conflict(string message = "Conflito com o estado atual do recurso.", IReadOnlyDictionary<string, string[]>? errors = null)
        => new(HttpStatusCode.Conflict, message, errors);

    public static HttpException TooManyRequests(string message = "Muitas requisições. Tente novamente mais tarde.")
        => new(HttpStatusCode.TooManyRequests, message);
}
