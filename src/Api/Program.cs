using Microsoft.EntityFrameworkCore;
using CoeurApi.Api.Extensions;
using CoeurApi.Api.Pages;
using CoeurApi.Infrastructure;
using CoeurApi.Infrastructure.Persistence;
using CoeurApi.Modules.Authentication;
using CoeurApi.Modules.Shopping;
using CoeurApi.Modules.Users;
using Scalar.AspNetCore;

namespace CoeurApi.Api;

public static class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddInfrastructure(builder.Configuration);
        builder.Services.AddOpenApi();

        builder.Services.AddUsersModule();
        builder.Services.AddAuthModule(builder.Configuration);
        builder.Services.AddShoppingModule();

        builder.Services.AddApiServices(builder.Configuration);

        var app = builder.Build();

        // Desligável via config (Database:AutoMigrate=false) pra ambientes com múltiplas
        // réplicas, onde migration deve rodar como step separado do deploy, não no startup.
        if (app.Configuration.GetValue("Database:AutoMigrate", true))
        {
            using var scope = app.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await db.Database.MigrateAsync();
        }

        app.UseApiServices();

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
