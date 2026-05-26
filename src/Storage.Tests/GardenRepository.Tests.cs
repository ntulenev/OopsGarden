using FluentAssertions;

using Microsoft.Data.Sqlite;

using Models;

using Storage.Repositories;

namespace Storage.Tests;

public sealed class PersistenceRepositoryTests
{
    [Fact(DisplayName = "Persistence repositories adds and lists locations and plants")]
    [Trait("Category", "Unit")]
    public async Task PersistenceRepositoryWhenGardenItemsAreAddedListsThem()
    {
        // Arrange
        var cancellationToken = new CancellationToken();
        await using var db = StorageTestContextFactory.CreateDbContext();
        var plants = new PlantRepository(db);
        var locations = new LocationRepository(db);
        var queries = new GardenQueries(db);
        var wateringHistory = new PlantWateringHistory(db);
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
        await locations.AddLocationAsync(location, cancellationToken);
        await plants.AddPlantAsync(plant, cancellationToken);
        await plants.AddWateringEventAsync(plant.Water(), cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        var locationResults = await queries.ListLocationsAsync(user.Id, cancellationToken);
        var plantResults = await queries.ListPlantsAsync(user.Id, cancellationToken);

        // Assert
        locationResults.Should().ContainSingle();
        locationResults[0].Name.Should().Be("Kitchen");
        locationResults[0].Plants.Should().Be(1);
        plantResults.Should().ContainSingle();
        plantResults[0].Name.Should().Be("Basil");
        plantResults[0].Location!.Name.Should().Be("Kitchen");
        plantResults[0].LastWateredAt.Should().NotBeNull();
    }

    [Fact(DisplayName = "Persistence repositories finds plant and location by owner")]
    [Trait("Category", "Unit")]
    public async Task PersistenceRepositoryWhenItemsExistFindsThemByOwner()
    {
        // Arrange
        var cancellationToken = new CancellationToken();
        await using var db = StorageTestContextFactory.CreateDbContext();
        var plants = new PlantRepository(db);
        var locations = new LocationRepository(db);
        var queries = new GardenQueries(db);
        var wateringHistory = new PlantWateringHistory(db);
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
        await locations.AddLocationAsync(location, cancellationToken);
        await plants.AddPlantAsync(plant, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);

        // Act
        var foundLocation = await locations.FindLocationAsync(user.Id, location.Id, cancellationToken);
        var foundPlant = await plants.FindPlantAsync(user.Id, plant.Id, cancellationToken);
        var locationExists = await locations.LocationExistsAsync(user.Id, location.Id, cancellationToken);

        // Assert
        foundLocation.Should().NotBeNull();
        foundLocation!.Name.Should().Be(location.Name);
        foundPlant.Should().NotBeNull();
        foundPlant!.Name.Should().Be(plant.Name);
        locationExists.Should().BeTrue();
    }

    [Fact(DisplayName = "Persistence repositories removes tracked plant and location")]
    [Trait("Category", "Unit")]
    public async Task PersistenceRepositoryWhenTrackedItemsAreRemovedDeletesEntities()
    {
        // Arrange
        var cancellationToken = new CancellationToken();
        await using var db = StorageTestContextFactory.CreateDbContext();
        var plants = new PlantRepository(db);
        var locations = new LocationRepository(db);
        var queries = new GardenQueries(db);
        var wateringHistory = new PlantWateringHistory(db);
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
        await locations.AddLocationAsync(location, cancellationToken);
        await plants.AddPlantAsync(plant, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);

        // Act
        plants.RemovePlant(plant);
        locations.RemoveLocation(location);
        await db.SaveChangesAsync(cancellationToken);

        // Assert
        db.Plants.Should().BeEmpty();
        db.Locations.Should().BeEmpty();
    }

    [Fact(DisplayName = "Persistence repositories clears location and replaces watering history")]
    [Trait("Category", "Unit")]
    public async Task PersistenceRepositoryWhenPlantHistoryChangesPersistsChanges()
    {
        // Arrange
        var cancellationToken = new CancellationToken();
        await using var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync(cancellationToken);
        await using var db = StorageTestContextFactory.CreateSqliteDbContext(connection);
        await db.Database.EnsureCreatedAsync(cancellationToken);
        var plants = new PlantRepository(db);
        var locations = new LocationRepository(db);
        var queries = new GardenQueries(db);
        var wateringHistory = new PlantWateringHistory(db);
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
        await locations.AddLocationAsync(location, cancellationToken);
        await plants.AddPlantAsync(plant, cancellationToken);
        await plants.AddWateringEventAsync(plant.Water(), cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        var lastWateredOn = new DateOnly(2026, 5, 22);

        // Act
        await locations.ClearPlantLocationAsync(user.Id, location.Id, cancellationToken);
        await wateringHistory.ReplaceAsync(plant.Id, lastWateredOn, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        db.ChangeTracker.Clear();
        var updatedPlant = db.Plants.Single(entity => entity.Id == plant.Id.Value);
        var watering = db.WateringEvents.Single(entity => entity.PlantId == plant.Id.Value);

        // Assert
        updatedPlant.LocationId.Should().BeNull();
        watering.WateredAt.Date.Should().Be(lastWateredOn.ToDateTime(TimeOnly.MinValue).Date);
    }

    [Fact(DisplayName = "Persistence repositories removes watering history when replacement date is missing")]
    [Trait("Category", "Unit")]
    public async Task PersistenceRepositoryWhenLastWateredOnIsMissingClearsWateringHistory()
    {
        // Arrange
        var cancellationToken = new CancellationToken();
        await using var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync(cancellationToken);
        await using var db = StorageTestContextFactory.CreateSqliteDbContext(connection);
        await db.Database.EnsureCreatedAsync(cancellationToken);
        var plants = new PlantRepository(db);
        var locations = new LocationRepository(db);
        var queries = new GardenQueries(db);
        var wateringHistory = new PlantWateringHistory(db);
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
        await plants.AddPlantAsync(plant, cancellationToken);
        await plants.AddWateringEventAsync(plant.Water(), cancellationToken);
        await db.SaveChangesAsync(cancellationToken);

        // Act
        await wateringHistory.ReplaceAsync(plant.Id, null, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);

        // Assert
        db.WateringEvents.Should().BeEmpty();
    }

    [Fact(DisplayName = "Persistence repositories returns public garden only when user is public and active")]
    [Trait("Category", "Unit")]
    public async Task PersistenceRepositoryWhenGardenIsPublicReturnsPublicGarden()
    {
        // Arrange
        var cancellationToken = new CancellationToken();
        await using var db = StorageTestContextFactory.CreateDbContext();
        var plants = new PlantRepository(db);
        var locations = new LocationRepository(db);
        var queries = new GardenQueries(db);
        var wateringHistory = new PlantWateringHistory(db);
        var publicUser = StorageTestContextFactory.CreateUser("public@example.com", isGardenPublic: true);
        var privateUser = StorageTestContextFactory.CreateUser("private@example.com");
        db.Users.Add(publicUser.ToEntity());
        db.Users.Add(privateUser.ToEntity());
        var publicPlant = Plant.Create(
            publicUser.Id,
            PlantName.From("Basil"),
            PlantDescription.From("Green"),
            null,
            null,
            null);
        db.Plants.Add(publicPlant.ToEntity());
        await db.SaveChangesAsync(cancellationToken);

        // Act
        var publicGarden = await queries.GetPublicGardenAsync(publicUser.Id, cancellationToken);
        var privateGarden = await queries.GetPublicGardenAsync(privateUser.Id, cancellationToken);
        var publicPlantExists = await queries.PublicPlantExistsAsync(publicUser.Id, publicPlant.Id, cancellationToken);
        var privatePlantExists = await queries.PublicPlantExistsAsync(privateUser.Id, publicPlant.Id, cancellationToken);

        // Assert
        publicGarden.Should().NotBeNull();
        publicGarden!.Plants.Should().ContainSingle();
        privateGarden.Should().BeNull();
        publicPlantExists.Should().BeTrue();
        privatePlantExists.Should().BeFalse();
    }

    [Fact(DisplayName = "Persistence repositories adds, counts, and lists plant notes")]
    [Trait("Category", "Unit")]
    public async Task PersistenceRepositoryWhenPlantNotesExistListsPagedNotesForOwner()
    {
        // Arrange
        var cancellationToken = new CancellationToken();
        await using var db = StorageTestContextFactory.CreateDbContext();
        var plants = new PlantRepository(db);
        var locations = new LocationRepository(db);
        var queries = new GardenQueries(db);
        var wateringHistory = new PlantWateringHistory(db);
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
        await plants.AddPlantAsync(plant, cancellationToken);
        var olderNote = PlantNote.Restore(
            PlantNoteId.New(),
            plant.Id,
            PlantNoteText.From("Older"),
            false,
            DateTimeOffset.UtcNow.AddDays(-1));
        var newerNote = PlantNote.Restore(
            PlantNoteId.New(),
            plant.Id,
            PlantNoteText.From("Newer"),
            false,
            DateTimeOffset.UtcNow);

        // Act
        await plants.AddPlantNoteAsync(olderNote, cancellationToken);
        await plants.AddPlantNoteAsync(newerNote, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        var total = await queries.CountPlantNotesAsync(user.Id, plant.Id, cancellationToken);
        var notes = await queries.ListPlantNotesAsync(user.Id, plant.Id, 0, 1, cancellationToken);

        // Assert
        total.Should().Be(2);
        notes.Should().ContainSingle();
        notes[0].Id.Should().Be(newerNote.Id);
        notes[0].Text.Should().Be("Newer");
    }

    [Fact(DisplayName = "Persistence repositories removes plant note only for owner")]
    [Trait("Category", "Unit")]
    public async Task PersistenceRepositoryWhenPlantNoteIsRemovedChecksOwnerAndPlant()
    {
        // Arrange
        var cancellationToken = new CancellationToken();
        await using var db = StorageTestContextFactory.CreateDbContext();
        var plants = new PlantRepository(db);
        var locations = new LocationRepository(db);
        var queries = new GardenQueries(db);
        var wateringHistory = new PlantWateringHistory(db);
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
        await plants.AddPlantAsync(plant, cancellationToken);
        var note = plant.AddNote(PlantNoteText.From("Sprouted"));
        await plants.AddPlantNoteAsync(note, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);

        // Act
        var otherUserDeleted = await plants.RemovePlantNoteAsync(otherUser.Id, plant.Id, note.Id, cancellationToken);
        var ownerDeleted = await plants.RemovePlantNoteAsync(owner.Id, plant.Id, note.Id, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);

        // Assert
        otherUserDeleted.Should().BeFalse();
        ownerDeleted.Should().BeTrue();
        db.PlantNotes.Should().BeEmpty();
    }
}


