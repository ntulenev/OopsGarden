using Abstractions.Repositories;

using FluentAssertions;

using Logic.UseCases;

using Models;

using Moq;

namespace OopsGarden.Tests;

public sealed class UpdatePlantUseCaseTests
{
    [Fact(DisplayName = "Update plant returns not found for missing plant")]
    [Trait("Category", "Unit")]
    public async Task UpdatePlantWhenPlantIsMissingReturnsNotFound()
    {
        // Arrange
        var cancellationToken = new CancellationToken();
        var userId = UserId.New();
        var plantId = PlantId.New();
        var plantsMock = new Mock<IPlantRepository>(MockBehavior.Strict);
        var unitOfWorkMock = TestUnitOfWorkFactory.Create(plants: plantsMock.Object);
        plantsMock.Setup(repo => repo.FindPlantAsync(userId, plantId, cancellationToken)).ReturnsAsync((Plant?)null);
        var useCase = new UpdatePlantUseCase(unitOfWorkMock.Object, new TestClock());

        // Act
        var result = await useCase.ExecuteAsync(
            userId,
            plantId,
            new PlantCommand("Basil", "Green", null, null, null, null),
            cancellationToken);

        // Assert
        result.Status.Should().Be(UpdatePlantStatus.NotFound);
    }

    [Fact(DisplayName = "Update plant returns invalid for missing location")]
    [Trait("Category", "Unit")]
    public async Task UpdatePlantWhenLocationIsMissingReturnsInvalid()
    {
        // Arrange
        var cancellationToken = new CancellationToken();
        var userId = UserId.New();
        var plant = Plant.Create(userId, PlantName.From("Basil"), PlantDescription.From(null), null, null, null);
        var locationId = LocationId.New();
        var plantsMock = new Mock<IPlantRepository>(MockBehavior.Strict);
        var locationsMock = new Mock<ILocationRepository>(MockBehavior.Strict);
        var unitOfWorkMock = TestUnitOfWorkFactory.Create(plants: plantsMock.Object, locations: locationsMock.Object);

        plantsMock.Setup(repo => repo.FindPlantAsync(userId, plant.Id, cancellationToken)).ReturnsAsync(plant);
        locationsMock.Setup(repo => repo.LocationExistsAsync(userId, locationId, cancellationToken)).ReturnsAsync(false);
        var useCase = new UpdatePlantUseCase(unitOfWorkMock.Object, new TestClock());

        // Act
        var result = await useCase.ExecuteAsync(
            userId,
            plant.Id,
            new PlantCommand("Basil", "Green", locationId.Value, null, null, null),
            cancellationToken);

        // Assert
        result.Status.Should().Be(UpdatePlantStatus.Invalid);
        result.Error.Should().Be(PlantCommandError.InvalidLocation);
        result.ErrorMessage.Should().Be("Invalid location.");
    }

    [Fact(DisplayName = "Update plant updates details and appends watering history")]
    [Trait("Category", "Unit")]
    public async Task UpdatePlantWhenCommandIsValidUpdatesDetailsAndAppendsWateringHistory()
    {
        // Arrange
        var cancellationToken = new CancellationToken();
        var userId = UserId.New();
        var plant = Plant.Create(userId, PlantName.From("Basil"), PlantDescription.From(null), null, null, null);
        var lastWateredOn = new DateOnly(2026, 5, 22);
        var plantsMock = new Mock<IPlantRepository>(MockBehavior.Strict);
        var unitOfWorkMock = TestUnitOfWorkFactory.Create(plants: plantsMock.Object);
        var wateringCalls = 0;
        var noteTexts = new List<string>();
        var saveCalls = 0;

        plantsMock.Setup(repo => repo.FindPlantAsync(userId, plant.Id, cancellationToken)).ReturnsAsync(plant);
        plantsMock
            .Setup(repo => repo.AddWateringEventAsync(
                It.Is<WateringEvent>(watering =>
                    watering.PlantId == plant.Id &&
                    watering.WateredAt == new DateTimeOffset(2026, 5, 22, 12, 0, 0, TimeSpan.Zero)),
                cancellationToken))
            .Callback(() => wateringCalls++)
            .Returns(Task.CompletedTask);
        plantsMock
            .Setup(repo => repo.AddPlantNoteAsync(It.IsAny<PlantNote>(), cancellationToken))
            .Callback<PlantNote, CancellationToken>((note, _) => noteTexts.Add(note.Text.Value))
            .Returns(Task.CompletedTask);
        unitOfWorkMock
            .Setup(work => work.SaveChangesAsync(cancellationToken))
            .Callback(() => saveCalls++)
            .Returns(Task.CompletedTask);

        var useCase = new UpdatePlantUseCase(unitOfWorkMock.Object, new TestClock());

        // Act
        var result = await useCase.ExecuteAsync(
            userId,
            plant.Id,
            new PlantCommand("Mint", "Fresh", null, null, lastWateredOn, null),
            cancellationToken);

        // Assert
        result.Status.Should().Be(UpdatePlantStatus.Updated);
        plant.Name.Value.Should().Be("Mint");
        wateringCalls.Should().Be(1);
        noteTexts.Should().Equal(
            "Name changed \"Basil\" -> \"Mint\"",
            "Description changed \"\" -> \"Fresh\"");
        saveCalls.Should().Be(1);
    }

    [Fact(DisplayName = "Update plant records photo history when photo changes")]
    [Trait("Category", "Unit")]
    public async Task UpdatePlantWhenPhotoChangesRecordsPhotoHistory()
    {
        // Arrange
        var cancellationToken = new CancellationToken();
        var userId = UserId.New();
        var oldPhoto = "data:image/png;base64,old";
        var newPhoto = "data:image/png;base64,new";
        var clock = new TestClock();
        var plant = Plant.Create(userId, PlantName.From("Basil"), PlantDescription.From(null), null, null, oldPhoto);
        var plantsMock = new Mock<IPlantRepository>(MockBehavior.Strict);
        var unitOfWorkMock = TestUnitOfWorkFactory.Create(plants: plantsMock.Object);
        var photoCalls = 0;
        var noteCalls = 0;

        plantsMock.Setup(repo => repo.FindPlantAsync(userId, plant.Id, cancellationToken)).ReturnsAsync(plant);
        plantsMock
            .Setup(repo => repo.AddPlantPhotoAsync(
                plant.Id,
                It.Is<ImageDataUrl>(photo => photo.Value == newPhoto),
                clock.UtcNow,
                cancellationToken))
            .Callback(() => photoCalls++)
            .Returns(Task.CompletedTask);
        plantsMock
            .Setup(repo => repo.AddPlantNoteAsync(It.IsAny<PlantNote>(), cancellationToken))
            .Callback(() => noteCalls++)
            .Returns(Task.CompletedTask);
        unitOfWorkMock
            .Setup(work => work.SaveChangesAsync(cancellationToken))
            .Returns(Task.CompletedTask);

        var useCase = new UpdatePlantUseCase(unitOfWorkMock.Object, clock);

        // Act
        var result = await useCase.ExecuteAsync(
            userId,
            plant.Id,
            new PlantCommand("Basil", "Green", null, null, null, newPhoto),
            cancellationToken);

        // Assert
        result.Status.Should().Be(UpdatePlantStatus.Updated);
        plant.PhotoDataUrl?.Value.Should().Be(newPhoto);
        photoCalls.Should().Be(1);
        noteCalls.Should().Be(1);
    }
}
