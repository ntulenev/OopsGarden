using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

using Models;

namespace Storage.Tests;

internal static class StorageTestContextFactory
{
    public static GardenDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<GardenDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new GardenDbContext(options);
    }

    public static GardenDbContext CreateSqliteDbContext(SqliteConnection connection)
    {
        var options = new DbContextOptionsBuilder<GardenDbContext>()
            .UseSqlite(connection)
            .Options;
        return new GardenDbContext(options);
    }

    public static AppUser CreateUser(string email, bool isGardenPublic = false) =>
        AppUser.Restore(
            UserId.New(),
            UserEmail.From(email),
            DisplayName.From("User"),
            PasswordHash.From("hash"),
            LanguageCode.From("en"),
            null,
            isGardenPublic,
            isBlocked: false,
            DateTimeOffset.UtcNow);
}
