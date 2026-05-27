using Microsoft.EntityFrameworkCore;
using NeonVertexApi.App.Core.Database;
using NeonVertexApi.App.Core.Extensions;
using NeonVertexApi.App.Modules.Users;
using Scalar.AspNetCore;

namespace NeonVertexApi;

public static class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddCore(builder.Configuration);
        builder.Services.AddUsersModule();
        builder.Services.AddControllers();
        builder.Services.AddOpenApi();

        var app = builder.Build();

        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await db.Database.MigrateAsync();
        }

        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            app.MapScalarApiReference();
        }

        app.UseHttpsRedirection();
        app.UseAuthorization();
        app.MapControllers();

        await app.RunAsync();
    }
}