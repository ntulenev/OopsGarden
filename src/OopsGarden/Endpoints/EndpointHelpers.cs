using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

using Models;

namespace OopsGarden.Endpoints;

internal static class EndpointHelpers
{
    public static UserId CurrentUserId(this ClaimsPrincipal principal)
    {
        var value = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(value, out var id)
            ? UserId.From(id)
            : throw new InvalidOperationException("Missing user id.");
    }

    public static async Task SignInUserAsync(this HttpContext httpContext, AppUser user)
    {
        ArgumentNullException.ThrowIfNull(httpContext);
        ArgumentNullException.ThrowIfNull(user);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.Value.ToString()),
            new(ClaimTypes.Name, user.DisplayName.Value),
            new(ClaimTypes.Email, user.Email.Value),
            new(ClaimTypes.Role, "User"),
            new("language", user.Language.Value)
        };

        if (user.AvatarDataUrl is not null)
        {
            claims.Add(new Claim("avatar", user.AvatarDataUrl.Value.Value));
        }

        await httpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme)),
            new AuthenticationProperties { IsPersistent = true });
    }

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
