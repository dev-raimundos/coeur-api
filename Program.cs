using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using NeonVertexApi.App.Core.Database;
using NeonVertexApi.App.Core.Extensions;
using NeonVertexApi.App.Core.Pages;
using NeonVertexApi.App.Modules.Authentication;
using NeonVertexApi.App.Modules.Shopping;
using NeonVertexApi.App.Modules.Users;
using Scalar.AspNetCore;

namespace NeonVertexApi;

public static class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddCore(builder.Configuration);
        builder.Services.AddOpenApi();

        if (builder.Environment.IsProduction())
        {
            builder.Services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders = ForwardedHeaders.XForwardedProto;
                options.KnownProxies.Clear();
            });
        }

        builder.Services.AddUsersModule();
        builder.Services.AddAuthModule();
        builder.Services.AddShoppingModule();

        var app = builder.Build();

        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await db.Database.MigrateAsync();
        }

        if (app.Environment.IsProduction())
        {
            app.UseForwardedHeaders();
            app.UseHttpsRedirection();
        }

        app.UseCore();

        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            app.MapScalarApiReference();
            app.MapGet("/scalar", () => Results.Redirect("/scalar/v1"));
        }

        app.MapGet("/", (IWebHostEnvironment env) =>
            Results.Content(StatusPage.Render(env), "text/html"));

        app.MapControllers();

        await app.RunAsync();
    }
}
