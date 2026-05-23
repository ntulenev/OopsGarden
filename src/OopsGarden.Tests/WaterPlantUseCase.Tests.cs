using Abstractions.Repositories;

using FluentAssertions;

using Logic.UseCases;

using Models;

using Moq;

namespace OopsGarden.Tests;

public sealed class WaterPlantUseCaseTests
{
    [Fact(DisplayName = "Water plant adds watering event")]
    [Trait("Category", "Unit")]
    public async Task WaterPlantWhenPlantExistsAddsWateringEvent()
    {
        // Arrange
        var cancellationToken = new CancellationToken();
        var userId = UserId.New();
        var plant = Plant.Create(userId, PlantName.From("Basil"), PlantDescription.From(null), null, null, null);
        var gardenMock = new Mock<IGardenRepository>(MockBehavior.Strict);
        var unitOfWorkMock = TestUnitOfWorkFactory.Create(garden: gardenMock.Object);
        var wateringCalls = 0;
        var saveCalls = 0;

        gardenMock.Setup(repo => repo.FindPlantAsync(userId, plant.Id, cancellationToken)).ReturnsAsync(plant);
        gardenMock
            .Setup(repo => repo.AddWateringEventAsync(It.Is<WateringEvent>(watering => watering.PlantId == plant.Id), cancellationToken))
            .Callback(() => wateringCalls++)
            .Returns(Task.CompletedTask);
        unitOfWorkMock.Setup(work => work.SaveChangesAsync(cancellationToken)).Callback(() => saveCalls++).Returns(Task.CompletedTask);
        var useCase = new WaterPlantUseCase(unitOfWorkMock.Object, new TestClock());

        // Act
        var result = await useCase.ExecuteAsync(userId, plant.Id, cancellationToken);

        // Assert
        result.Should().NotBeNull();
        wateringCalls.Should().Be(1);
        saveCalls.Should().Be(1);
    }

    [Fact(DisplayName = "Water plant returns null for missing plant")]
    [Trait("Category", "Unit")]
    public async Task WaterPlantWhenPlantIsMissingReturnsNull()
    {
        // Arrange
        var cancellationToken = new CancellationToken();
        var userId = UserId.New();
        var plantId = PlantId.New();
        var gardenMock = new Mock<IGardenRepository>(MockBehavior.Strict);
        var unitOfWorkMock = TestUnitOfWorkFactory.Create(garden: gardenMock.Object);
        gardenMock.Setup(repo => repo.FindPlantAsync(userId, plantId, cancellationToken)).ReturnsAsync((Plant?)null);
        var useCase = new WaterPlantUseCase(unitOfWorkMock.Object, new TestClock());

        // Act
        var result = await useCase.ExecuteAsync(userId, plantId, cancellationToken);

        // Assert
        result.Should().BeNull();
    }
}
