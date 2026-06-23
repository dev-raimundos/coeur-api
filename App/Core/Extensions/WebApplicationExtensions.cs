using NeonVertexApi.App.Core.Middleware;

namespace NeonVertexApi.App.Core.Extensions;

public static class WebApplicationExtensions
{
    public static WebApplication UseCore(this WebApplication app)
    {
        // Registers the global exception handler middleware.
        // Must be the first middleware in the pipeline to catch exceptions
        // thrown by any subsequent middleware or controller.
        app.UseMiddleware<ExceptionMiddleware>();

        // Enables CORS policy for the Angular frontend.
        // Must be called before UseAuthentication and UseAuthorization.
        app.UseCors("Frontend");

        // Enables the authentication middleware, which reads the JWT from the
        // HTTP-only cookie and populates HttpContext.User with the token claims.
        app.UseAuthentication();

        // Authorization is enforced via AuthorizeFilter on MVC controllers
        // (registered in ServiceCollectionExtensions). Keeping UseAuthorization()
        // out of the pipeline ensures minimal-API endpoints (e.g. Scalar, OpenAPI)
        // are never challenged, without needing AllowAnonymous on each one.

        return app;
    }
}