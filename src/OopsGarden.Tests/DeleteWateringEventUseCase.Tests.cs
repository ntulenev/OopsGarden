using Abstractions.Repositories;

using FluentAssertions;

using Logic.UseCases;

using Models;

using Moq;

namespace OopsGarden.Tests;

public sealed class DeleteWateringEventUseCaseTests
{
    [Fact(DisplayName = "Delete watering event returns false when event is missing")]
    [Trait("Category", "Unit")]
    public async Task DeleteWateringEventWhenEventIsMissingReturnsFalse()
    {
        // Arrange
        var cancellationToken = new CancellationToken();
        var userId = UserId.New();
        var plantId = PlantId.New();
        var wateringId = WateringEventId.New();
        var wateringEventsMock = new Mock<IWateringEventRepository>(MockBehavior.Strict);
        var unitOfWorkMock = TestUnitOfWorkFactory.Create(wateringEvents: wateringEventsMock.Object);

        wateringEventsMock
            .Setup(repo => repo.RemoveWateringEventAsync(userId, plantId, wateringId, cancellationToken))
            .ReturnsAsync(false);

        var useCase = new DeleteWateringEventUseCase(unitOfWorkMock.Object);

        // Act
        var result = await useCase.ExecuteAsync(userId, plantId, wateringId, cancellationToken);

        // Assert
        result.Status.Should().Be(CommandStatus.NotFound);
    }

    [Fact(DisplayName = "Delete watering event saves when event is removed")]
    [Trait("Category", "Unit")]
    public async Task DeleteWateringEventWhenEventIsRemovedSaves()
    {
        // Arrange
        var cancellationToken = new CancellationToken();
        var userId = UserId.New();
        var plantId = PlantId.New();
        var wateringId = WateringEventId.New();
        var wateringEventsMock = new Mock<IWateringEventRepository>(MockBehavior.Strict);
        var unitOfWorkMock = TestUnitOfWorkFactory.Create(wateringEvents: wateringEventsMock.Object);
        var saveCalls = 0;

        wateringEventsMock
            .Setup(repo => repo.RemoveWateringEventAsync(userId, plantId, wateringId, cancellationToken))
            .ReturnsAsync(true);
        unitOfWorkMock
            .Setup(work => work.SaveChangesAsync(cancellationToken))
            .Callback(() => saveCalls++)
            .Returns(Task.CompletedTask);

        var useCase = new DeleteWateringEventUseCase(unitOfWorkMock.Object);

        // Act
        var result = await useCase.ExecuteAsync(userId, plantId, wateringId, cancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        saveCalls.Should().Be(1);
    }
}
