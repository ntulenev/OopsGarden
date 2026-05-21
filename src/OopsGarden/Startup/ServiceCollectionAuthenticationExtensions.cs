using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

using Models;
using Storage;

namespace OopsGarden.Startup;

internal static class ServiceCollectionAuthenticationExtensions
{
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
        var db = context.HttpContext.RequestServices.GetRequiredService<GardenDbContext>();
        var isBlocked = await db.Users
            .Where(user => user.Id == userId)
            .Select(user => user.IsBlocked)
            .SingleOrDefaultAsync(context.HttpContext.RequestAborted)
            .ConfigureAwait(false);

        if (isBlocked)
        {
            context.RejectPrincipal();
        }
    }
}
