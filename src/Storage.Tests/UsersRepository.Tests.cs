using FluentAssertions;

using Models;

using Storage.Repositories;

namespace Storage.Tests;

public sealed class UsersRepositoryTests
{
    [Fact(DisplayName = "UsersRepository adds and finds user by email")]
    [Trait("Category", "Unit")]
    public async Task UsersRepositoryWhenUserIsAddedFindsByEmail()
    {
        // Arrange
        var cancellationToken = new CancellationToken();
        await using var db = StorageTestContextFactory.CreateDbContext();
        var repository = new UsersRepository(db);
        var user = StorageTestContextFactory.CreateUser("user@example.com");

        // Act
        await repository.AddAsync(user, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        var found = await repository.FindByEmailAsync(UserEmail.From("USER@example.com"), cancellationToken);

        // Assert
        found.Should().NotBeNull();
        found!.Id.Should().Be(user.Id);
        found.Email.Value.Should().Be("USER@EXAMPLE.COM");
    }

    [Fact(DisplayName = "UsersRepository syncs tracked user changes")]
    [Trait("Category", "Unit")]
    public async Task UsersRepositoryWhenTrackedUserChangesSyncsEntity()
    {
        // Arrange
        var cancellationToken = new CancellationToken();
        await using var db = StorageTestContextFactory.CreateDbContext();
        var repository = new UsersRepository(db);
        var user = StorageTestContextFactory.CreateUser("user@example.com");
        await repository.AddAsync(user, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);

        // Act
        var found = await repository.FindByIdAsync(user.Id, cancellationToken);
        found!.Block();
        repository.SyncChanges();
        await db.SaveChangesAsync(cancellationToken);

        // Assert
        db.Users.Single(entity => entity.Id == user.Id.Value).IsBlocked.Should().BeTrue();
    }
}
