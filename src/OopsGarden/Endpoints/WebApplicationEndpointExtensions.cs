using System.Security.Claims;

namespace OopsGarden.Endpoints;

internal static class WebApplicationEndpointExtensions
{
    public static WebApplication MapOopsGardenEndpoints(this WebApplication app)
    {
        ArgumentNullException.ThrowIfNull(app);

        _ = app.MapGet("/api/me", (ClaimsPrincipal principal) =>
        {
            if (!principal.Identity?.IsAuthenticated ?? true)
            {
                return Results.Ok(new { authenticated = false });
            }

            return Results.Ok(new
            {
                authenticated = true,
                id = principal.FindFirstValue(ClaimTypes.NameIdentifier),
                name = principal.Identity?.Name,
                role = principal.FindFirstValue(ClaimTypes.Role),
                language = principal.FindFirstValue("language") ?? "en",
                avatar = principal.FindFirstValue("avatar")
            });
        });

        app.MapAuthEndpoints();
        app.MapAdminEndpoints();
        app.MapGardenEndpoints();

        return app;
    }
}
