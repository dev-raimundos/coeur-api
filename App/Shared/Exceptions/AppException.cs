namespace NeonVertexApi.App.Shared.Exceptions;

public class AppException : Exception
{
    public int StatusCode { get; }
    public IReadOnlyDictionary<string, string[]>? Errors { get; }

    private AppException(int statusCode, string message, IReadOnlyDictionary<string, string[]>? errors = null) : base(message)
    {
        StatusCode = statusCode;
        Errors = errors;
    }

    // -------------------------------------------------------------------------
    // 2xx
    // -------------------------------------------------------------------------

    public static AppException NoContent(string message = "Sem conteúdo.")
        => new(204, message);

    // -------------------------------------------------------------------------
    // 4xx
    // -------------------------------------------------------------------------

    public static AppException BadRequest(string message = "Requisição inválida.", IReadOnlyDictionary<string, string[]>? errors = null)
        => new(400, message, errors);

    public static AppException Unauthorized(string message = "Não autenticado.")
        => new(401, message);

    public static AppException PaymentRequired(string message = "Pagamento necessário.")
        => new(402, message);

    public static AppException Forbidden(string message = "Acesso negado.")
        => new(403, message);

    public static AppException NotFound(string message = "Recurso não encontrado.")
        => new(404, message);

    public static AppException MethodNotAllowed(string message = "Método não permitido.")
        => new(405, message);

    public static AppException NotAcceptable(string message = "Formato não aceito.")
        => new(406, message);

    public static AppException Conflict(string message = "Conflito com o estado atual do recurso.", IReadOnlyDictionary<string, string[]>? errors = null)
        => new(409, message, errors);

    public static AppException Gone(string message = "Recurso não está mais disponível.")
        => new(410, message);

    public static AppException UnprocessableEntity(string message = "Não foi possível processar a requisição.", IReadOnlyDictionary<string, string[]>? errors = null)
        => new(422, message, errors);

    public static AppException Locked(string message = "Recurso bloqueado.")
        => new(423, message);

    public static AppException TooManyRequests(string message = "Muitas requisições. Tente novamente mais tarde.")
        => new(429, message);

    // -------------------------------------------------------------------------
    // 5xx
    // -------------------------------------------------------------------------

    public static AppException InternalError(string message = "Erro interno do servidor.")
        => new(500, message);

    public static AppException NotImplemented(string message = "Funcionalidade não implementada.")
        => new(501, message);

    public static AppException BadGateway(string message = "Resposta inválida do servidor upstream.")
        => new(502, message);

    public static AppException ServiceUnavailable(string message = "Serviço indisponível.")
        => new(503, message);

    public static AppException GatewayTimeout(string message = "Tempo de resposta do servidor upstream esgotado.")
        => new(504, message);

    // -------------------------------------------------------------------------
    // Toast
    // -------------------------------------------------------------------------

    public string ToastType => StatusCode switch
    {
        >= 500 => "error",
        >= 400 => "warning",
        _ => "info"
    };
}