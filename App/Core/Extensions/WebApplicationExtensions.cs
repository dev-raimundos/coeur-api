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
        // Must be called before UseAuthorization.
        app.UseAuthentication();

        // Enables the authorization middleware, which enforces [Authorize]
        // attributes on controllers and endpoints based on the authenticated user.
        app.UseAuthorization();

        return app;
    }
}