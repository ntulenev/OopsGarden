using System.Security.Claims;

using Abstractions.Repositories;

using Microsoft.AspNetCore.Authentication.Cookies;

using Models;

namespace OopsGarden.Startup;

/// <summary>
/// Provides authentication service registration extensions.
/// </summary>
internal static class ServiceCollectionAuthenticationExtensions
{
    /// <summary>
    /// Registers OopsGarden authentication services.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The same service collection.</returns>
    public static IServiceCollection AddOopsGardenAuthentication(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        _ = services
            .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(options =>
            {
                options.Cookie.Name = "OopsGarden.Auth";
                options.LoginPath = "/admin";
                options.AccessDeniedPath = "/";
                options.SlidingExpiration = true;
                options.Events.OnValidatePrincipal = ValidateUserPrincipalAsync;
                options.Events.OnRedirectToLogin = context =>
                {
                    if (context.Request.Path.StartsWithSegments("/api", StringComparison.OrdinalIgnoreCase))
                    {
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        return Task.CompletedTask;
                    }

                    context.Response.Redirect(context.RedirectUri);
                    return Task.CompletedTask;
                };
                options.Events.OnRedirectToAccessDenied = context =>
                {
                    if (context.Request.Path.StartsWithSegments("/api", StringComparison.OrdinalIgnoreCase))
                    {
                        context.Response.StatusCode = StatusCodes.Status403Forbidden;
                        return Task.CompletedTask;
                    }

                    context.Response.Redirect(context.RedirectUri);
                    return Task.CompletedTask;
                };
            });

        _ = services.AddAuthorization();

        return services;
    }

    private static async Task ValidateUserPrincipalAsync(CookieValidatePrincipalContext context)
    {
        var role = context.Principal?.FindFirstValue(ClaimTypes.Role);
        if (role == "Admin")
        {
            return;
        }

        var idValue = context.Principal?.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(idValue, out var rawUserId))
        {
            context.RejectPrincipal();
            return;
        }

        var userId = UserId.From(rawUserId);
        var unitOfWork = context.HttpContext.RequestServices.GetRequiredService<IUnitOfWork>();
        var user = await unitOfWork.Users
            .FindByIdAsync(userId, context.HttpContext.RequestAborted)
            .ConfigureAwait(false);

        if (user is null || user.IsBlocked)
        {
            context.RejectPrincipal();
        }
    }
}
