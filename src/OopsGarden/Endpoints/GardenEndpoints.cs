namespace OopsGarden.Endpoints;

/// <summary>
/// Maps garden endpoints.
/// </summary>
internal static class GardenEndpoints
{
    /// <summary>
    /// Maps garden HTTP endpoints.
    /// </summary>
    /// <param name="app">The web application.</param>
    public static void MapGardenEndpoints(this WebApplication app)
    {
        app.MapPublicGardenEndpoints();

        var group = app.MapGroup("/api/garden").RequireAuthorization(policy => policy.RequireRole("User"));
        group.MapGardenPlantEndpoints();
        group.MapGardenPlantNoteEndpoints();
        group.MapGardenLocationEndpoints();
    }
}
