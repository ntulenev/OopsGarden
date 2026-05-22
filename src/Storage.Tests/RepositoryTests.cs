using FluentAssertions;

using Microsoft.Data.Sqlite;
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
        var cancellationToken = new CancellationToken();
        await using var db = CreateDbContext();
        var repository = new UsersRepository(db);
        var user = CreateUser("user@example.com");

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
        await using var db = CreateDbContext();
        var repository = new UsersRepository(db);
        var user = CreateUser("user@example.com");
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

    [Fact(DisplayName = "InvitesRepository adds lists and removes invites")]
    [Trait("Category", "Unit")]
    public async Task InvitesRepositoryWhenInviteIsAddedListsAndRemovesInvite()
    {
        // Arrange
        var cancellationToken = new CancellationToken();
        await using var db = CreateDbContext();
        var repository = new InvitesRepository(db);
        var invite = InviteLink.Create(InviteCode.From("code"), AdminName.From("admin"));

        // Act
        await repository.AddAsync(invite, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        var list = await repository.ListAsync(cancellationToken);
        repository.Remove(invite);
        await db.SaveChangesAsync(cancellationToken);
        var afterRemove = await repository.FindByIdAsync(invite.Id, cancellationToken);

        // Assert
        list.Should().ContainSingle();
        list[0].Code.Should().Be("code");
        afterRemove.Should().BeNull();
    }

    [Fact(DisplayName = "InvitesRepository finds invite by code and id")]
    [Trait("Category", "Unit")]
    public async Task InvitesRepositoryWhenInviteExistsFindsByCodeAndId()
    {
        // Arrange
        var cancellationToken = new CancellationToken();
        await using var db = CreateDbContext();
        var repository = new InvitesRepository(db);
        var invite = InviteLink.Create(InviteCode.From("code"), AdminName.From("admin"));
        await repository.AddAsync(invite, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);

        // Act
        var byCode = await repository.FindByCodeAsync(InviteCode.From("code"), cancellationToken);
        var byId = await repository.FindByIdAsync(invite.Id, cancellationToken);

        // Assert
        byCode.Should().NotBeNull();
        byCode!.Id.Should().Be(invite.Id);
        byId.Should().NotBeNull();
        byId!.Code.Should().Be(invite.Code);
    }

    [Fact(DisplayName = "GardenRepository adds and lists locations and plants")]
    [Trait("Category", "Unit")]
    public async Task GardenRepositoryWhenGardenItemsAreAddedListsThem()
    {
        // Arrange
        var cancellationToken = new CancellationToken();
        await using var db = CreateDbContext();
        var repository = new GardenRepository(db);
        var user = CreateUser("user@example.com");
        db.Users.Add(user.ToEntity());
        await db.SaveChangesAsync(cancellationToken);
        var location = Location.Create(user.Id, LocationName.From("Kitchen"));
        var plant = Plant.Create(
            user.Id,
            PlantName.From("Basil"),
            PlantDescription.From("Green"),
            location.Id,
            new DateOnly(2026, 5, 22),
            null);

        // Act
        await repository.AddLocationAsync(location, cancellationToken);
        await repository.AddPlantAsync(plant, cancellationToken);
        await repository.AddWateringEventAsync(plant.Water(), cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        var locations = await repository.ListLocationsAsync(user.Id, cancellationToken);
        var plants = await repository.ListPlantsAsync(user.Id, cancellationToken);

        // Assert
        locations.Should().ContainSingle();
        locations[0].Name.Should().Be("Kitchen");
        locations[0].Plants.Should().Be(1);
        plants.Should().ContainSingle();
        plants[0].Name.Should().Be("Basil");
        plants[0].Location!.Name.Should().Be("Kitchen");
        plants[0].LastWateredAt.Should().NotBeNull();
    }

    [Fact(DisplayName = "GardenRepository finds plant and location by owner")]
    [Trait("Category", "Unit")]
    public async Task GardenRepositoryWhenItemsExistFindsThemByOwner()
    {
        // Arrange
        var cancellationToken = new CancellationToken();
        await using var db = CreateDbContext();
        var repository = new GardenRepository(db);
        var user = CreateUser("user@example.com");
        db.Users.Add(user.ToEntity());
        await db.SaveChangesAsync(cancellationToken);
        var location = Location.Create(user.Id, LocationName.From("Kitchen"));
        var plant = Plant.Create(
            user.Id,
            PlantName.From("Basil"),
            PlantDescription.From("Green"),
            location.Id,
            null,
            null);
        await repository.AddLocationAsync(location, cancellationToken);
        await repository.AddPlantAsync(plant, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);

        // Act
        var foundLocation = await repository.FindLocationAsync(user.Id, location.Id, cancellationToken);
        var foundPlant = await repository.FindPlantAsync(user.Id, plant.Id, cancellationToken);
        var locationExists = await repository.LocationExistsAsync(user.Id, location.Id, cancellationToken);

        // Assert
        foundLocation.Should().NotBeNull();
        foundLocation!.Name.Should().Be(location.Name);
        foundPlant.Should().NotBeNull();
        foundPlant!.Name.Should().Be(plant.Name);
        locationExists.Should().BeTrue();
    }

    [Fact(DisplayName = "GardenRepository removes tracked plant and location")]
    [Trait("Category", "Unit")]
    public async Task GardenRepositoryWhenTrackedItemsAreRemovedDeletesEntities()
    {
        // Arrange
        var cancellationToken = new CancellationToken();
        await using var db = CreateDbContext();
        var repository = new GardenRepository(db);
        var user = CreateUser("user@example.com");
        db.Users.Add(user.ToEntity());
        await db.SaveChangesAsync(cancellationToken);
        var location = Location.Create(user.Id, LocationName.From("Kitchen"));
        var plant = Plant.Create(
            user.Id,
            PlantName.From("Basil"),
            PlantDescription.From("Green"),
            location.Id,
            null,
            null);
        await repository.AddLocationAsync(location, cancellationToken);
        await repository.AddPlantAsync(plant, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);

        // Act
        repository.RemovePlant(plant);
        repository.RemoveLocation(location);
        await db.SaveChangesAsync(cancellationToken);

        // Assert
        db.Plants.Should().BeEmpty();
        db.Locations.Should().BeEmpty();
    }

    [Fact(DisplayName = "GardenRepository clears location and replaces watering history")]
    [Trait("Category", "Unit")]
    public async Task GardenRepositoryWhenPlantHistoryChangesPersistsChanges()
    {
        // Arrange
        var cancellationToken = new CancellationToken();
        await using var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync(cancellationToken);
        await using var db = CreateSqliteDbContext(connection);
        await db.Database.EnsureCreatedAsync(cancellationToken);
        var repository = new GardenRepository(db);
        var user = CreateUser("user@example.com");
        db.Users.Add(user.ToEntity());
        await db.SaveChangesAsync(cancellationToken);
        var location = Location.Create(user.Id, LocationName.From("Kitchen"));
        var plant = Plant.Create(
            user.Id,
            PlantName.From("Basil"),
            PlantDescription.From("Green"),
            location.Id,
            null,
            null);
        await repository.AddLocationAsync(location, cancellationToken);
        await repository.AddPlantAsync(plant, cancellationToken);
        await repository.AddWateringEventAsync(plant.Water(), cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        var lastWateredOn = new DateOnly(2026, 5, 22);

        // Act
        await repository.ClearPlantLocationAsync(user.Id, location.Id, cancellationToken);
        await repository.ReplaceWateringHistoryAsync(plant.Id, lastWateredOn, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        db.ChangeTracker.Clear();
        var updatedPlant = db.Plants.Single(entity => entity.Id == plant.Id.Value);
        var watering = db.WateringEvents.Single(entity => entity.PlantId == plant.Id.Value);

        // Assert
        updatedPlant.LocationId.Should().BeNull();
        watering.WateredAt.Date.Should().Be(lastWateredOn.ToDateTime(TimeOnly.MinValue).Date);
    }

    [Fact(DisplayName = "GardenRepository removes watering history when replacement date is missing")]
    [Trait("Category", "Unit")]
    public async Task GardenRepositoryWhenLastWateredOnIsMissingClearsWateringHistory()
    {
        // Arrange
        var cancellationToken = new CancellationToken();
        await using var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync(cancellationToken);
        await using var db = CreateSqliteDbContext(connection);
        await db.Database.EnsureCreatedAsync(cancellationToken);
        var repository = new GardenRepository(db);
        var user = CreateUser("user@example.com");
        db.Users.Add(user.ToEntity());
        await db.SaveChangesAsync(cancellationToken);
        var plant = Plant.Create(
            user.Id,
            PlantName.From("Basil"),
            PlantDescription.From("Green"),
            null,
            null,
            null);
        await repository.AddPlantAsync(plant, cancellationToken);
        await repository.AddWateringEventAsync(plant.Water(), cancellationToken);
        await db.SaveChangesAsync(cancellationToken);

        // Act
        await repository.ReplaceWateringHistoryAsync(plant.Id, null, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);

        // Assert
        db.WateringEvents.Should().BeEmpty();
    }

    [Fact(DisplayName = "GardenRepository returns public garden only when user is public and active")]
    [Trait("Category", "Unit")]
    public async Task GardenRepositoryWhenGardenIsPublicReturnsPublicGarden()
    {
        // Arrange
        var cancellationToken = new CancellationToken();
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
        await db.SaveChangesAsync(cancellationToken);

        // Act
        var publicGarden = await repository.GetPublicGardenAsync(publicUser.Id, cancellationToken);
        var privateGarden = await repository.GetPublicGardenAsync(privateUser.Id, cancellationToken);

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
        var cancellationToken = new CancellationToken();
        await using var db = CreateDbContext();
        var users = new UsersRepository(db);
        var invites = new InvitesRepository(db);
        var garden = new GardenRepository(db);
        var unitOfWork = new EfUnitOfWork(db, users, invites, garden);
        var user = CreateUser("user@example.com");

        // Act
        await unitOfWork.Users.AddAsync(user, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

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

    private static GardenDbContext CreateSqliteDbContext(SqliteConnection connection)
    {
        var options = new DbContextOptionsBuilder<GardenDbContext>()
            .UseSqlite(connection)
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
