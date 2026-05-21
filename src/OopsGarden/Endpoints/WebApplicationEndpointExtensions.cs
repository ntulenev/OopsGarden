using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

using Storage;

namespace OopsGarden.Endpoints;

internal static class WebApplicationEndpointExtensions
{
    public static WebApplication MapOopsGardenEndpoints(this WebApplication app)
    {
        ArgumentNullException.ThrowIfNull(app);

        _ = app.MapGet("/api/me", async (ClaimsPrincipal principal, GardenDbContext db) =>
        {
            if (!principal.Identity?.IsAuthenticated ?? true)
            {
                return Results.Ok(new { authenticated = false });
            }

            var role = principal.FindFirstValue(ClaimTypes.Role);
            if (role == "Admin")
            {
                return Results.Ok(new
                {
                    authenticated = true,
                    id = principal.FindFirstValue(ClaimTypes.NameIdentifier),
                    name = principal.Identity?.Name,
                    role,
                    language = principal.FindFirstValue("language") ?? "en",
                    avatar = (string?)null,
                    isGardenPublic = false
                });
            }

            var userId = principal.CurrentUserId();
            var user = await db.Users
                .Where(user => user.Id == userId)
                .Select(user => new
                {
                    id = user.Id.Value,
                    name = user.DisplayName.Value,
                    language = user.Language.Value,
                    avatar = user.AvatarDataUrl == null ? null : user.AvatarDataUrl.Value.Value,
                    isGardenPublic = user.IsGardenPublic
                })
                .SingleOrDefaultAsync();

            if (user is null)
            {
                return Results.Unauthorized();
            }

            return Results.Ok(new
            {
                authenticated = true,
                user.id,
                user.name,
                role,
                user.language,
                user.avatar,
                user.isGardenPublic
            });
        });

        app.MapAuthEndpoints();
        app.MapAdminEndpoints();
        app.MapGardenEndpoints();

        return app;
    }
}
