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
        var plant = Plant.Create(userId, PlantName.From("Basil"), PlantDescription.From(null), null, null, null);
        var plantsMock = new Mock<IPlantRepository>(MockBehavior.Strict);
        var unitOfWorkMock = TestUnitOfWorkFactory.Create(plants: plantsMock.Object);

        plantsMock
            .Setup(repo => repo.FindPlantAsync(userId, plantId, cancellationToken))
            .ReturnsAsync(plant);
        plantsMock
            .Setup(repo => repo.FindPlantPhotoAsync(userId, plantId, photoId, cancellationToken))
            .ReturnsAsync((PlantPhotoSnapshot?)null);

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
        var plant = Plant.Create(userId, PlantName.From("Basil"), PlantDescription.From(null), null, null, null);
        var photo = new PlantPhotoSnapshot(photoId, plantId, "data:image/png;base64,photo", DateTimeOffset.UtcNow);
        var plantsMock = new Mock<IPlantRepository>(MockBehavior.Strict);
        var unitOfWorkMock = TestUnitOfWorkFactory.Create(plants: plantsMock.Object);
        var saveCalls = 0;

        plantsMock
            .Setup(repo => repo.FindPlantAsync(userId, plantId, cancellationToken))
            .ReturnsAsync(plant);
        plantsMock
            .Setup(repo => repo.FindPlantPhotoAsync(userId, plantId, photoId, cancellationToken))
            .ReturnsAsync(photo);
        plantsMock
            .Setup(repo => repo.FindLatestPlantPhotoAsync(userId, plantId, null, cancellationToken))
            .ReturnsAsync(photo);
        plantsMock
            .Setup(repo => repo.FindLatestPlantPhotoAsync(userId, plantId, photoId, cancellationToken))
            .ReturnsAsync((PlantPhotoSnapshot?)null);
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

    [Fact(DisplayName = "Delete plant photo restores previous photo when latest is removed")]
    [Trait("Category", "Unit")]
    public async Task DeletePlantPhotoWhenLatestPhotoIsRemovedRestoresPreviousPhoto()
    {
        // Arrange
        var cancellationToken = new CancellationToken();
        var userId = UserId.New();
        var plantId = PlantId.New();
        var photoId = Guid.NewGuid();
        var previousPhotoId = Guid.NewGuid();
        var plant = Plant.Restore(
            plantId,
            userId,
            PlantName.From("Basil"),
            PlantDescription.From(null),
            PlantSoil.From(null),
            null,
            null,
            ImageDataUrl.PlantPhoto("data:image/png;base64,current"),
            default);
        var currentPhoto = new PlantPhotoSnapshot(
            photoId,
            plantId,
            "data:image/png;base64,current",
            DateTimeOffset.UtcNow);
        var previousPhoto = new PlantPhotoSnapshot(
            previousPhotoId,
            plantId,
            "data:image/png;base64,previous",
            DateTimeOffset.UtcNow.AddDays(-1));
        var plantsMock = new Mock<IPlantRepository>(MockBehavior.Strict);
        var unitOfWorkMock = TestUnitOfWorkFactory.Create(plants: plantsMock.Object);

        plantsMock.Setup(repo => repo.FindPlantAsync(userId, plantId, cancellationToken)).ReturnsAsync(plant);
        plantsMock.Setup(repo => repo.FindPlantPhotoAsync(userId, plantId, photoId, cancellationToken)).ReturnsAsync(currentPhoto);
        plantsMock.Setup(repo => repo.FindLatestPlantPhotoAsync(userId, plantId, null, cancellationToken)).ReturnsAsync(currentPhoto);
        plantsMock.Setup(repo => repo.FindLatestPlantPhotoAsync(userId, plantId, photoId, cancellationToken)).ReturnsAsync(previousPhoto);
        plantsMock.Setup(repo => repo.RemovePlantPhotoAsync(userId, plantId, photoId, cancellationToken)).ReturnsAsync(true);
        unitOfWorkMock.Setup(work => work.SaveChangesAsync(cancellationToken)).Returns(Task.CompletedTask);

        var useCase = new DeletePlantPhotoUseCase(unitOfWorkMock.Object);

        // Act
        var result = await useCase.ExecuteAsync(userId, plantId, photoId, cancellationToken);

        // Assert
        result.Should().BeTrue();
        plant.PhotoDataUrl?.Value.Should().Be("data:image/png;base64,previous");
    }
}
