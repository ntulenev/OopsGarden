using FluentAssertions;

using Microsoft.Data.Sqlite;

using Models;

using Storage.Repositories;

namespace Storage.Tests;

public sealed class GardenRepositoryTests
{
    [Fact(DisplayName = "GardenRepository adds and lists locations and plants")]
    [Trait("Category", "Unit")]
    public async Task GardenRepositoryWhenGardenItemsAreAddedListsThem()
    {
        // Arrange
        var cancellationToken = new CancellationToken();
        await using var db = StorageTestContextFactory.CreateDbContext();
        var repository = new GardenRepository(db);
        var user = StorageTestContextFactory.CreateUser("user@example.com");
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
        await using var db = StorageTestContextFactory.CreateDbContext();
        var repository = new GardenRepository(db);
        var user = StorageTestContextFactory.CreateUser("user@example.com");
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
        await using var db = StorageTestContextFactory.CreateDbContext();
        var repository = new GardenRepository(db);
        var user = StorageTestContextFactory.CreateUser("user@example.com");
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
        await using var db = StorageTestContextFactory.CreateSqliteDbContext(connection);
        await db.Database.EnsureCreatedAsync(cancellationToken);
        var repository = new GardenRepository(db);
        var user = StorageTestContextFactory.CreateUser("user@example.com");
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
        await repository.ReplaceAsync(plant.Id, lastWateredOn, cancellationToken);
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
        await using var db = StorageTestContextFactory.CreateSqliteDbContext(connection);
        await db.Database.EnsureCreatedAsync(cancellationToken);
        var repository = new GardenRepository(db);
        var user = StorageTestContextFactory.CreateUser("user@example.com");
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
        await repository.ReplaceAsync(plant.Id, null, cancellationToken);
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
        await using var db = StorageTestContextFactory.CreateDbContext();
        var repository = new GardenRepository(db);
        var publicUser = StorageTestContextFactory.CreateUser("public@example.com", isGardenPublic: true);
        var privateUser = StorageTestContextFactory.CreateUser("private@example.com");
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

    [Fact(DisplayName = "GardenRepository adds, counts, and lists plant notes")]
    [Trait("Category", "Unit")]
    public async Task GardenRepositoryWhenPlantNotesExistListsPagedNotesForOwner()
    {
        // Arrange
        var cancellationToken = new CancellationToken();
        await using var db = StorageTestContextFactory.CreateDbContext();
        var repository = new GardenRepository(db);
        var user = StorageTestContextFactory.CreateUser("user@example.com");
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
        var olderNote = PlantNote.Restore(
            PlantNoteId.New(),
            plant.Id,
            PlantNoteText.From("Older"),
            DateTimeOffset.UtcNow.AddDays(-1));
        var newerNote = PlantNote.Restore(
            PlantNoteId.New(),
            plant.Id,
            PlantNoteText.From("Newer"),
            DateTimeOffset.UtcNow);

        // Act
        await repository.AddPlantNoteAsync(olderNote, cancellationToken);
        await repository.AddPlantNoteAsync(newerNote, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        var total = await repository.CountPlantNotesAsync(user.Id, plant.Id, cancellationToken);
        var notes = await repository.ListPlantNotesAsync(user.Id, plant.Id, 0, 1, cancellationToken);

        // Assert
        total.Should().Be(2);
        notes.Should().ContainSingle();
        notes[0].Id.Should().Be(newerNote.Id);
        notes[0].Text.Should().Be("Newer");
    }

    [Fact(DisplayName = "GardenRepository removes plant note only for owner")]
    [Trait("Category", "Unit")]
    public async Task GardenRepositoryWhenPlantNoteIsRemovedChecksOwnerAndPlant()
    {
        // Arrange
        var cancellationToken = new CancellationToken();
        await using var db = StorageTestContextFactory.CreateDbContext();
        var repository = new GardenRepository(db);
        var owner = StorageTestContextFactory.CreateUser("owner@example.com");
        var otherUser = StorageTestContextFactory.CreateUser("other@example.com");
        db.Users.Add(owner.ToEntity());
        db.Users.Add(otherUser.ToEntity());
        await db.SaveChangesAsync(cancellationToken);
        var plant = Plant.Create(
            owner.Id,
            PlantName.From("Basil"),
            PlantDescription.From("Green"),
            null,
            null,
            null);
        await repository.AddPlantAsync(plant, cancellationToken);
        var note = plant.AddNote(PlantNoteText.From("Sprouted"));
        await repository.AddPlantNoteAsync(note, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);

        // Act
        var otherUserDeleted = await repository.RemovePlantNoteAsync(otherUser.Id, plant.Id, note.Id, cancellationToken);
        var ownerDeleted = await repository.RemovePlantNoteAsync(owner.Id, plant.Id, note.Id, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);

        // Assert
        otherUserDeleted.Should().BeFalse();
        ownerDeleted.Should().BeTrue();
        db.PlantNotes.Should().BeEmpty();
    }
}
