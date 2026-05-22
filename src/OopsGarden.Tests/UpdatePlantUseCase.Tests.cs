
using FluentAssertions;

using Models;

using Moq;

using OopsGarden.UseCases;

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
        var gardenMock = new Mock<IGardenRepository>(MockBehavior.Strict);
        var unitOfWorkMock = TestUnitOfWorkFactory.Create(garden: gardenMock.Object);
        gardenMock.Setup(repo => repo.FindPlantAsync(userId, plantId, cancellationToken)).ReturnsAsync((Plant?)null);
        var useCase = new UpdatePlantUseCase(unitOfWorkMock.Object);

        // Act
        var result = await useCase.ExecuteAsync(
            userId,
            plantId.Value,
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
        var gardenMock = new Mock<IGardenRepository>(MockBehavior.Strict);
        var unitOfWorkMock = TestUnitOfWorkFactory.Create(garden: gardenMock.Object);

        gardenMock.Setup(repo => repo.FindPlantAsync(userId, plant.Id, cancellationToken)).ReturnsAsync(plant);
        gardenMock.Setup(repo => repo.LocationExistsAsync(userId, locationId, cancellationToken)).ReturnsAsync(false);
        var useCase = new UpdatePlantUseCase(unitOfWorkMock.Object);

        // Act
        var result = await useCase.ExecuteAsync(
            userId,
            plant.Id.Value,
            new PlantCommand("Basil", "Green", locationId.Value, null, null, null),
            cancellationToken);

        // Assert
        result.Status.Should().Be(UpdatePlantStatus.Invalid);
        result.Error.Should().Be("Invalid location.");
    }

    [Fact(DisplayName = "Update plant updates details and watering history")]
    [Trait("Category", "Unit")]
    public async Task UpdatePlantWhenCommandIsValidUpdatesDetailsAndWateringHistory()
    {
        // Arrange
        var cancellationToken = new CancellationToken();
        var userId = UserId.New();
        var plant = Plant.Create(userId, PlantName.From("Basil"), PlantDescription.From(null), null, null, null);
        var lastWateredOn = new DateOnly(2026, 5, 22);
        var gardenMock = new Mock<IGardenRepository>(MockBehavior.Strict);
        var unitOfWorkMock = TestUnitOfWorkFactory.Create(garden: gardenMock.Object);
        var replaceCalls = 0;
        var saveCalls = 0;

        gardenMock.Setup(repo => repo.FindPlantAsync(userId, plant.Id, cancellationToken)).ReturnsAsync(plant);
        gardenMock
            .Setup(repo => repo.ReplaceWateringHistoryAsync(plant.Id, lastWateredOn, cancellationToken))
            .Callback(() => replaceCalls++)
            .Returns(Task.CompletedTask);
        unitOfWorkMock
            .Setup(work => work.SaveChangesAsync(cancellationToken))
            .Callback(() => saveCalls++)
            .Returns(Task.CompletedTask);

        var useCase = new UpdatePlantUseCase(unitOfWorkMock.Object);

        // Act
        var result = await useCase.ExecuteAsync(
            userId,
            plant.Id.Value,
            new PlantCommand("Mint", "Fresh", null, null, lastWateredOn, null),
            cancellationToken);

        // Assert
        result.Status.Should().Be(UpdatePlantStatus.Updated);
        plant.Name.Value.Should().Be("Mint");
        replaceCalls.Should().Be(1);
        saveCalls.Should().Be(1);
    }
}
