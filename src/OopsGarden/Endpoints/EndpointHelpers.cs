using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

using Models;

namespace OopsGarden.Endpoints;

/// <summary>
/// Provides helpers shared by HTTP endpoints.
/// </summary>
internal static class EndpointHelpers
{
    /// <summary>
    /// Gets the current authenticated user id.
    /// </summary>
    /// <param name="principal">The authenticated principal.</param>
    /// <returns>The current user id.</returns>
    public static UserId CurrentUserId(this ClaimsPrincipal principal)
    {
        var value = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(value, out var id)
            ? UserId.From(id)
            : throw new InvalidOperationException("Missing user id.");
    }

    /// <summary>
    /// Signs in an application user.
    /// </summary>
    /// <param name="httpContext">The current HTTP context.</param>
    /// <param name="user">The authenticated user.</param>
    /// <returns>A task that represents the sign-in operation.</returns>
    public static async Task SignInUserAsync(this HttpContext httpContext, AuthenticatedUser user)
    {
        ArgumentNullException.ThrowIfNull(httpContext);
        ArgumentNullException.ThrowIfNull(user);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.Value.ToString()),
            new(ClaimTypes.Name, user.DisplayName),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, "User"),
            new("language", user.Language)
        };

        await httpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme)),
            new AuthenticationProperties { IsPersistent = true });
    }

    /// <summary>
    /// Signs in an administrator.
    /// </summary>
    /// <param name="httpContext">The current HTTP context.</param>
    /// <param name="userName">The administrator user name.</param>
    /// <returns>A task that represents the sign-in operation.</returns>
    public static async Task SignInAdminAsync(this HttpContext httpContext, string userName)
    {
        ArgumentNullException.ThrowIfNull(httpContext);
        ArgumentNullException.ThrowIfNull(userName);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, $"admin:{userName}"),
            new(ClaimTypes.Name, userName),
            new(ClaimTypes.Role, "Admin"),
            new("language", "en")
        };

        await httpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme)),
            new AuthenticationProperties { IsPersistent = true });
    }
}
