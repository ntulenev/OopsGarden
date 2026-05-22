using FluentAssertions;

using Storage.Repositories;

namespace Storage.Tests;

public sealed class EfUnitOfWorkTests
{
    [Fact(DisplayName = "EfUnitOfWork exposes repositories and saves tracked changes")]
    [Trait("Category", "Unit")]
    public async Task EfUnitOfWorkWhenSaveChangesIsCalledPersistsChanges()
    {
        // Arrange
        var cancellationToken = new CancellationToken();
        await using var db = StorageTestContextFactory.CreateDbContext();
        var users = new UsersRepository(db);
        var invites = new InvitesRepository(db);
        var garden = new GardenRepository(db);
        var unitOfWork = new EfUnitOfWork(db, users, invites, garden);
        var user = StorageTestContextFactory.CreateUser("user@example.com");

        // Act
        await unitOfWork.Users.AddAsync(user, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        // Assert
        unitOfWork.Users.Should().BeSameAs(users);
        unitOfWork.Invites.Should().BeSameAs(invites);
        unitOfWork.Garden.Should().BeSameAs(garden);
        db.Users.Should().ContainSingle(entity => entity.Id == user.Id.Value);
    }
}
