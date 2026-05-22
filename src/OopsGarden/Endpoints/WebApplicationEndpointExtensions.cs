using Abstractions;

namespace OopsGarden.Endpoints;

/// <summary>
/// Provides endpoint registration extensions for the web application.
/// </summary>
internal static class WebApplicationEndpointExtensions
{
    /// <summary>
    /// Maps all OopsGarden endpoints.
    /// </summary>
    /// <param name="app">The web application.</param>
    /// <returns>The same web application.</returns>
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
