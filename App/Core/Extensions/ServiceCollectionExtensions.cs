using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using NeonVertexApi.App.Core.Authentication;
using NeonVertexApi.App.Core.Database;
using NeonVertexApi.App.Core.Settings;
using NeonVertexApi.App.Shared.Interfaces;
using System.Text;

namespace NeonVertexApi.App.Core.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCore(this IServiceCollection services, IConfiguration configuration)
    {
        // ── Database ──────────────────────────────────────────────────────────
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("Default")));

        // ── JWT Settings ──────────────────────────────────────────────────────
        // Reads JWT configuration from appsettings / User Secrets / environment
        // variables and makes it available via IOptions<JwtSettings> in the DI
        // container.
        var jwtSettings = configuration.GetSection("Jwt").Get<JwtSettings>()!;
        services.Configure<JwtSettings>(configuration.GetSection("Jwt"));

        // ── CORS ──────────────────────────────────────────────────────────────────
        // Allows the Angular dev server to communicate with the API.
        // withCredentials is required for HTTP-only cookies to be sent cross-origin.
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

        // ── Authentication ────────────────────────────────────────────────────
        // Registers TokenService, responsible for generating JWT tokens.
        // Registers IHttpContextAccessor so services outside the HTTP pipeline
        // can access the HttpContext (used by CurrentUserService).
        // Registers CurrentUserService, which extracts the authenticated user's
        // data from the claims present in the request cookie.
        services.AddScoped<TokenService>();
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUser, CurrentUserService>();

        // Configures JWT Bearer as the default authentication scheme and sets
        // the token validation parameters:
        // - ValidateIssuer/Audience: ensures the token was issued by this API
        // - ValidateLifetime: rejects expired tokens
        // - ValidateIssuerSigningKey: verifies the signature using the configured secret
        // OnMessageReceived: overrides the default Authorization header lookup
        // to read the token from the "access_token" HTTP-only cookie, protecting
        // against XSS attacks since the cookie is not accessible via JavaScript.
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

        // ── Controllers ───────────────────────────────────────────────────────
        // Registers controllers with two global filters:
        // - ProducesAttribute: sets application/json as the default content-type
        // - AuthorizeFilter: requires authentication on all controller endpoints.
        // Using AuthorizeFilter (MVC-scoped) instead of SetFallbackPolicy (global)
        // ensures that minimal API endpoints such as /scalar and /openapi remain
        // accessible without authentication.
        services.AddControllers(options =>
        {
            options.Filters.Add(new ProducesAttribute("application/json"));
            options.Filters.Add(new AuthorizeFilter());
        });

        services.AddAuthorization();

        return services;
    }
}