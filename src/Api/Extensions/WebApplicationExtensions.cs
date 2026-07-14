namespace CoeurApi.Api.Extensions;

public static class WebApplicationExtensions
{
    public static WebApplication UseApiServices(this WebApplication app)
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
