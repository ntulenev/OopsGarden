using Abstractions.Repositories;

using FluentAssertions;

using Logic.UseCases;

using Models;

using Moq;

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
        var plantsMock = new Mock<IPlantRepository>(MockBehavior.Strict);
        var unitOfWorkMock = TestUnitOfWorkFactory.Create(plants: plantsMock.Object);
        var removeCalls = 0;
        var saveCalls = 0;

        plantsMock.Setup(repo => repo.FindPlantAsync(userId, plant.Id, cancellationToken)).ReturnsAsync(plant);
        plantsMock.Setup(repo => repo.RemovePlant(plant)).Callback(() => removeCalls++);
        unitOfWorkMock.Setup(work => work.SaveChangesAsync(cancellationToken)).Callback(() => saveCalls++).Returns(Task.CompletedTask);

        var useCase = new DeletePlantUseCase(unitOfWorkMock.Object);

        // Act
        var result = await useCase.ExecuteAsync(userId, plant.Id, cancellationToken);

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
        var plantsMock = new Mock<IPlantRepository>(MockBehavior.Strict);
        var unitOfWorkMock = TestUnitOfWorkFactory.Create(plants: plantsMock.Object);
        plantsMock.Setup(repo => repo.FindPlantAsync(userId, plantId, cancellationToken)).ReturnsAsync((Plant?)null);
        var useCase = new DeletePlantUseCase(unitOfWorkMock.Object);

        // Act
        var result = await useCase.ExecuteAsync(userId, plantId, cancellationToken);

        // Assert
        result.Should().BeFalse();
    }
}
