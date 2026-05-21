using Abstractions;

namespace OopsGarden.Endpoints;

internal static class WebApplicationEndpointExtensions
{
    public static WebApplication MapOopsGardenEndpoints(this WebApplication app)
    {
        ArgumentNullException.ThrowIfNull(app);

        _ = app.MapGet("/admin", async (IWebHostEnvironment environment) =>
            Results.Content(
                await File.ReadAllTextAsync(Path.Combine(environment.WebRootPath, "index.html")),
                "text/html"));

        _ = app.MapGet(
            "/api/me",
            async (IGetMeUseCase useCase, HttpContext http, CancellationToken cancellationToken) =>
                Results.Ok((await useCase.ExecuteAsync(http.User, cancellationToken).ConfigureAwait(false)).ToResponse()));

        app.MapAuthEndpoints();
        app.MapAdminEndpoints();
        app.MapGardenEndpoints();

        return app;
    }
}
