using Abstractions.Repositories;

using FluentAssertions;

using Logic.UseCases;

using Models;

using Moq;

namespace OopsGarden.Tests;

public sealed class DeletePlantPhotoUseCaseTests
{
    [Fact(DisplayName = "Delete plant photo returns false when photo is missing")]
    [Trait("Category", "Unit")]
    public async Task DeletePlantPhotoWhenPhotoIsMissingReturnsFalse()
    {
        // Arrange
        var cancellationToken = new CancellationToken();
        var userId = UserId.New();
        var plantId = PlantId.New();
        var photoId = Guid.NewGuid();
        var plantsMock = new Mock<IPlantRepository>(MockBehavior.Strict);
        var unitOfWorkMock = TestUnitOfWorkFactory.Create(plants: plantsMock.Object);

        plantsMock
            .Setup(repo => repo.RemovePlantPhotoAsync(userId, plantId, photoId, cancellationToken))
            .ReturnsAsync(false);

        var useCase = new DeletePlantPhotoUseCase(unitOfWorkMock.Object);

        // Act
        var result = await useCase.ExecuteAsync(userId, plantId, photoId, cancellationToken);

        // Assert
        result.Should().BeFalse();
    }

    [Fact(DisplayName = "Delete plant photo saves when photo is removed")]
    [Trait("Category", "Unit")]
    public async Task DeletePlantPhotoWhenPhotoIsRemovedSaves()
    {
        // Arrange
        var cancellationToken = new CancellationToken();
        var userId = UserId.New();
        var plantId = PlantId.New();
        var photoId = Guid.NewGuid();
        var plantsMock = new Mock<IPlantRepository>(MockBehavior.Strict);
        var unitOfWorkMock = TestUnitOfWorkFactory.Create(plants: plantsMock.Object);
        var saveCalls = 0;

        plantsMock
            .Setup(repo => repo.RemovePlantPhotoAsync(userId, plantId, photoId, cancellationToken))
            .ReturnsAsync(true);
        unitOfWorkMock
            .Setup(work => work.SaveChangesAsync(cancellationToken))
            .Callback(() => saveCalls++)
            .Returns(Task.CompletedTask);

        var useCase = new DeletePlantPhotoUseCase(unitOfWorkMock.Object);

        // Act
        var result = await useCase.ExecuteAsync(userId, plantId, photoId, cancellationToken);

        // Assert
        result.Should().BeTrue();
        saveCalls.Should().Be(1);
    }
}
