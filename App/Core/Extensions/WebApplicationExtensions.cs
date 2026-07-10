namespace CoeurApi.App.Core.Extensions;

public static class WebApplicationExtensions
{
    public static WebApplication UseCore(this WebApplication app)
    {
        app.UseForwardedHeaders();
        app.UseExceptionHandler();
        app.UseCors("Frontend");
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseRateLimiter();

        return app;
    }
}
