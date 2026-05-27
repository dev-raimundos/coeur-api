using NeonVertexApi.App.Core.Middleware;

namespace NeonVertexApi.App.Core.Extensions;

public static class WebApplicationExtensions
{
    public static WebApplication UseCore(this WebApplication app)
    {
        app.UseMiddleware<ExceptionMiddleware>();
        return app;
    }
}