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
        var plants = new PlantRepository(db);
        var locations = new LocationRepository(db);
        var gardenQueries = new GardenQueries(db);
        var unitOfWork = new EfUnitOfWork(db, users, invites, plants, locations, gardenQueries);
        var user = StorageTestContextFactory.CreateUser("user@example.com");

        // Act
        await unitOfWork.Users.AddAsync(user, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        // Assert
        unitOfWork.Users.Should().BeSameAs(users);
        unitOfWork.Invites.Should().BeSameAs(invites);
        unitOfWork.Plants.Should().BeSameAs(plants);
        unitOfWork.Locations.Should().BeSameAs(locations);
        unitOfWork.GardenQueries.Should().BeSameAs(gardenQueries);
        db.Users.Should().ContainSingle(entity => entity.Id == user.Id.Value);
    }
}
