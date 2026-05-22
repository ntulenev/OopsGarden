using Microsoft.EntityFrameworkCore;

using Storage;

namespace OopsGarden.Startup;

/// <summary>
/// Provides helpers for creating and running the web application.
/// </summary>
internal static class StartupHelpers
{
    /// <summary>
    /// Creates the configured OopsGarden web application.
    /// </summary>
    /// <param name="args">The command-line arguments.</param>
    /// <returns>The configured web application.</returns>
    public static WebApplication CreateApplication(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        _ = builder.Services.AddOopsGardenOptions(builder.Configuration);
        _ = builder.Services.AddOopsGardenStorage(builder.Configuration);
        _ = builder.Services.AddOopsGardenAuthentication();
        _ = builder.Services.AddOopsGardenApplicationServices();

        return builder.Build();
    }

    /// <summary>
    /// Applies startup tasks and runs the web application.
    /// </summary>
    /// <param name="app">The configured web application.</param>
    /// <returns>A task that completes when the application stops.</returns>
    public static async Task RunAppAsync(WebApplication app)
    {
        await app.EnsureDatabaseMigratedAsync().ConfigureAwait(false);
        await app.RunAsync().ConfigureAwait(false);
    }

    private static async Task EnsureDatabaseMigratedAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<GardenDbContext>();
        if (app.Environment.IsEnvironment("Testing"))
        {
            await db.Database.EnsureCreatedAsync().ConfigureAwait(false);
            return;
        }

        await db.Database.MigrateAsync().ConfigureAwait(false);
    }
}
