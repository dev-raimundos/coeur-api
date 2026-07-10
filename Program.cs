using Microsoft.EntityFrameworkCore;
using CoeurApi.App.Core.Database;
using CoeurApi.App.Core.Extensions;
using CoeurApi.App.Core.Pages;
using CoeurApi.App.Modules.Authentication;
using CoeurApi.App.Modules.Shopping;
using CoeurApi.App.Modules.Users;
using Scalar.AspNetCore;

namespace CoeurApi;

public static class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddCore(builder.Configuration);
        builder.Services.AddOpenApi();

        builder.Services.AddUsersModule();
        builder.Services.AddAuthModule();
        builder.Services.AddShoppingModule();

        var app = builder.Build();

        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await db.Database.MigrateAsync();
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
