using Microsoft.EntityFrameworkCore;

using Storage;

namespace OopsGarden.Startup;

internal static class StartupHelpers
{
    public static WebApplication CreateApplication(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        _ = builder.Services.AddOopsGardenOptions(builder.Configuration);
        _ = builder.Services.AddOopsGardenStorage(builder.Configuration);
        _ = builder.Services.AddOopsGardenAuthentication();
        _ = builder.Services.AddOopsGardenApplicationServices();

        return builder.Build();
    }

    public static async Task RunAppAsync(WebApplication app)
    {
        await app.EnsureDatabaseMigratedAsync().ConfigureAwait(false);
        await app.RunAsync().ConfigureAwait(false);
    }

    private static async Task EnsureDatabaseMigratedAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<GardenDbContext>();
        await db.Database.MigrateAsync().ConfigureAwait(false);
    }
}
