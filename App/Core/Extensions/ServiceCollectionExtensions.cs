using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using CoeurApi.App.Core.Authentication;
using CoeurApi.App.Core.Database;
using CoeurApi.App.Core.Filters;
using CoeurApi.App.Core.Settings;
using CoeurApi.App.Modules.Authentication.DTOs;
using CoeurApi.App.Modules.Authentication.Validators;
using CoeurApi.App.Modules.Shopping.DTOs;
using CoeurApi.App.Modules.Shopping.Validators;
using CoeurApi.App.Modules.Users.DTOs;
using CoeurApi.App.Modules.Users.Validators;
using CoeurApi.App.Shared.Interfaces;
using System.Text;
using System.Text.Json;
using System.Threading.RateLimiting;

namespace CoeurApi.App.Core.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCore(this IServiceCollection services, IConfiguration configuration)
    {
        // ── Database ──────────────────────────────────────────────────────────
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("Default")));

        // ── Forwarded Headers ────────────────────────────────────────────────
        // Confia no cabeçalho de proto/IP repassado pelo Cloudflare Tunnel (cloudflared),
        // que fala HTTP com o container dentro da rede interna do Docker.
        services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
            options.KnownIPNetworks.Clear();
            options.KnownProxies.Clear();
        });

        // ── JWT Settings ──────────────────────────────────────────────────────
        var jwtSettings = configuration.GetSection("Jwt").Get<JwtSettings>()!;
        services.Configure<JwtSettings>(configuration.GetSection("Jwt"));

        // ── CORS ──────────────────────────────────────────────────────────────
        services.AddCors(options =>
        {
            options.AddPolicy("Frontend", policy =>
            {
                policy.WithOrigins(
                          "http://localhost:4200",
                          "https://web-client-gold.vercel.app"
                      )
                      .AllowAnyHeader()
                      .AllowAnyMethod()
                      .AllowCredentials();
            });
        });

        // ── Rate Limiting ─────────────────────────────────────────────────────
        // Per-IP fixed window: 5 login attempts per minute.
        services.AddRateLimiter(options =>
        {
            options.AddPolicy("login", context =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        Window = TimeSpan.FromMinutes(1),
                        PermitLimit = 5,
                        QueueLimit = 0,
                        AutoReplenishment = true
                    }));

            options.OnRejected = async (ctx, cancellationToken) =>
            {
                ctx.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                ctx.HttpContext.Response.ContentType = "application/json";

                var body = JsonSerializer.Serialize(new
                {
                    message = "Muitas tentativas. Tente novamente em 1 minuto.",
                    toast = new { type = "warning", message = "Muitas tentativas. Tente novamente em 1 minuto." }
                }, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

                await ctx.HttpContext.Response.WriteAsync(body, cancellationToken);
            };
        });

        // ── Authentication ────────────────────────────────────────────────────
        services.AddScoped<TokenService>();
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUser, CurrentUserService>();

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings.Issuer,
                    ValidAudience = jwtSettings.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwtSettings.Secret))
                };

                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        context.Token = context.Request.Cookies["access_token"];
                        return Task.CompletedTask;
                    }
                };
            });

        // ── Validation ────────────────────────────────────────────────────────
        services.AddScoped<IValidator<LoginDto>, LoginDtoValidator>();
        services.AddScoped<IValidator<CreateUserDto>, CreateUserDtoValidator>();
        services.AddScoped<IValidator<UpdateUserDto>, UpdateUserDtoValidator>();
        services.AddScoped<IValidator<CreateProductDto>, CreateProductDtoValidator>();
        services.AddScoped<IValidator<UpdateProductDto>, UpdateProductDtoValidator>();

        // ── Controllers ───────────────────────────────────────────────────────
        services.AddControllers(options =>
        {
            options.Filters.Add(new ProducesAttribute("application/json"));
            options.Filters.Add(new AuthorizeFilter());
            options.Filters.Add<FluentValidationFilter>();
        });

        services.AddAuthorization();

        return services;
    }
}
