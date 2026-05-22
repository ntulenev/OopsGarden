using FluentAssertions;

using Microsoft.EntityFrameworkCore;

using Models;

using Storage.Repositories;

namespace Storage.Tests;

public sealed class RepositoryTests
{
    [Fact(DisplayName = "UsersRepository adds and finds user by email")]
    [Trait("Category", "Unit")]
    public async Task UsersRepositoryWhenUserIsAddedFindsByEmail()
    {
        // Arrange
        await using var db = CreateDbContext();
        var repository = new UsersRepository(db);
        var user = CreateUser("user@example.com");

        // Act
        await repository.AddAsync(user, CancellationToken.None);
        await db.SaveChangesAsync();
        var found = await repository.FindByEmailAsync(UserEmail.From("USER@example.com"), CancellationToken.None);

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
        await using var db = CreateDbContext();
        var repository = new UsersRepository(db);
        var user = CreateUser("user@example.com");
        await repository.AddAsync(user, CancellationToken.None);
        await db.SaveChangesAsync();

        // Act
        var found = await repository.FindByIdAsync(user.Id, CancellationToken.None);
        found!.Block();
        repository.SyncChanges();
        await db.SaveChangesAsync();

        // Assert
        db.Users.Single(entity => entity.Id == user.Id.Value).IsBlocked.Should().BeTrue();
    }

    [Fact(DisplayName = "InvitesRepository adds lists and removes invites")]
    [Trait("Category", "Unit")]
    public async Task InvitesRepositoryWhenInviteIsAddedListsAndRemovesInvite()
    {
        // Arrange
        await using var db = CreateDbContext();
        var repository = new InvitesRepository(db);
        var invite = InviteLink.Create(InviteCode.From("code"), AdminName.From("admin"));

        // Act
        await repository.AddAsync(invite, CancellationToken.None);
        await db.SaveChangesAsync();
        var list = await repository.ListAsync(CancellationToken.None);
        repository.Remove(invite);
        await db.SaveChangesAsync();
        var afterRemove = await repository.FindByIdAsync(invite.Id, CancellationToken.None);

        // Assert
        list.Should().ContainSingle();
        list[0].Code.Should().Be("code");
        afterRemove.Should().BeNull();
    }

    [Fact(DisplayName = "GardenRepository adds and lists locations and plants")]
    [Trait("Category", "Unit")]
    public async Task GardenRepositoryWhenGardenItemsAreAddedListsThem()
    {
        // Arrange
        await using var db = CreateDbContext();
        var repository = new GardenRepository(db);
        var user = CreateUser("user@example.com");
        db.Users.Add(user.ToEntity());
        await db.SaveChangesAsync();
        var location = Location.Create(user.Id, LocationName.From("Kitchen"));
        var plant = Plant.Create(
            user.Id,
            PlantName.From("Basil"),
            PlantDescription.From("Green"),
            location.Id,
            new DateOnly(2026, 5, 22),
            null);

        // Act
        await repository.AddLocationAsync(location, CancellationToken.None);
        await repository.AddPlantAsync(plant, CancellationToken.None);
        await repository.AddWateringEventAsync(plant.Water(), CancellationToken.None);
        await db.SaveChangesAsync();
        var locations = await repository.ListLocationsAsync(user.Id, CancellationToken.None);
        var plants = await repository.ListPlantsAsync(user.Id, CancellationToken.None);

        // Assert
        locations.Should().ContainSingle();
        locations[0].Name.Should().Be("Kitchen");
        locations[0].Plants.Should().Be(1);
        plants.Should().ContainSingle();
        plants[0].Name.Should().Be("Basil");
        plants[0].Location!.Name.Should().Be("Kitchen");
        plants[0].LastWateredAt.Should().NotBeNull();
    }

    [Fact(DisplayName = "GardenRepository returns public garden only when user is public and active")]
    [Trait("Category", "Unit")]
    public async Task GardenRepositoryWhenGardenIsPublicReturnsPublicGarden()
    {
        // Arrange
        await using var db = CreateDbContext();
        var repository = new GardenRepository(db);
        var publicUser = CreateUser("public@example.com", isGardenPublic: true);
        var privateUser = CreateUser("private@example.com");
        db.Users.Add(publicUser.ToEntity());
        db.Users.Add(privateUser.ToEntity());
        db.Plants.Add(Plant.Create(
            publicUser.Id,
            PlantName.From("Basil"),
            PlantDescription.From("Green"),
            null,
            null,
            null).ToEntity());
        await db.SaveChangesAsync();

        // Act
        var publicGarden = await repository.GetPublicGardenAsync(publicUser.Id, CancellationToken.None);
        var privateGarden = await repository.GetPublicGardenAsync(privateUser.Id, CancellationToken.None);

        // Assert
        publicGarden.Should().NotBeNull();
        publicGarden!.Plants.Should().ContainSingle();
        privateGarden.Should().BeNull();
    }

    [Fact(DisplayName = "EfUnitOfWork exposes repositories and saves tracked changes")]
    [Trait("Category", "Unit")]
    public async Task EfUnitOfWorkWhenSaveChangesIsCalledPersistsChanges()
    {
        // Arrange
        await using var db = CreateDbContext();
        var users = new UsersRepository(db);
        var invites = new InvitesRepository(db);
        var garden = new GardenRepository(db);
        var unitOfWork = new EfUnitOfWork(db, users, invites, garden);
        var user = CreateUser("user@example.com");

        // Act
        await unitOfWork.Users.AddAsync(user, CancellationToken.None);
        await unitOfWork.SaveChangesAsync(CancellationToken.None);

        // Assert
        unitOfWork.Users.Should().BeSameAs(users);
        unitOfWork.Invites.Should().BeSameAs(invites);
        unitOfWork.Garden.Should().BeSameAs(garden);
        db.Users.Should().ContainSingle(entity => entity.Id == user.Id.Value);
    }

    private static GardenDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<GardenDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new GardenDbContext(options);
    }

    private static AppUser CreateUser(string email, bool isGardenPublic = false) =>
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
