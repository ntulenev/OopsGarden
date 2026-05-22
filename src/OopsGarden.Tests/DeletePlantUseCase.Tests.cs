
using FluentAssertions;

using Models;

using Moq;

using Logic.UseCases;

namespace OopsGarden.Tests;

public sealed class DeletePlantUseCaseTests
{
    [Fact(DisplayName = "Delete plant removes existing plant")]
    [Trait("Category", "Unit")]
    public async Task DeletePlantWhenPlantExistsRemovesPlant()
    {
        // Arrange
        var cancellationToken = new CancellationToken();
        var userId = UserId.New();
        var plant = Plant.Create(userId, PlantName.From("Basil"), PlantDescription.From(null), null, null, null);
        var gardenMock = new Mock<IGardenRepository>(MockBehavior.Strict);
        var unitOfWorkMock = TestUnitOfWorkFactory.Create(garden: gardenMock.Object);
        var removeCalls = 0;
        var saveCalls = 0;

        gardenMock.Setup(repo => repo.FindPlantAsync(userId, plant.Id, cancellationToken)).ReturnsAsync(plant);
        gardenMock.Setup(repo => repo.RemovePlant(plant)).Callback(() => removeCalls++);
        unitOfWorkMock.Setup(work => work.SaveChangesAsync(cancellationToken)).Callback(() => saveCalls++).Returns(Task.CompletedTask);

        var useCase = new DeletePlantUseCase(unitOfWorkMock.Object);

        // Act
        var result = await useCase.ExecuteAsync(userId, plant.Id.Value, cancellationToken);

        // Assert
        result.Should().BeTrue();
        removeCalls.Should().Be(1);
        saveCalls.Should().Be(1);
    }

    [Fact(DisplayName = "Delete plant returns false for missing plant")]
    [Trait("Category", "Unit")]
    public async Task DeletePlantWhenPlantIsMissingReturnsFalse()
    {
        // Arrange
        var cancellationToken = new CancellationToken();
        var userId = UserId.New();
        var plantId = PlantId.New();
        var gardenMock = new Mock<IGardenRepository>(MockBehavior.Strict);
        var unitOfWorkMock = TestUnitOfWorkFactory.Create(garden: gardenMock.Object);
        gardenMock.Setup(repo => repo.FindPlantAsync(userId, plantId, cancellationToken)).ReturnsAsync((Plant?)null);
        var useCase = new DeletePlantUseCase(unitOfWorkMock.Object);

        // Act
        var result = await useCase.ExecuteAsync(userId, plantId.Value, cancellationToken);

        // Assert
        result.Should().BeFalse();
    }
}
