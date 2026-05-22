using Abstractions;

using FluentAssertions;

using Models;

using Moq;

using OopsGarden.UseCases;

namespace OopsGarden.Tests;

public sealed class DeletePlantNoteUseCaseTests
{
    [Fact(DisplayName = "Delete plant note returns false when note is missing")]
    [Trait("Category", "Unit")]
    public async Task DeletePlantNoteWhenNoteIsMissingReturnsFalse()
    {
        // Arrange
        var cancellationToken = new CancellationToken();
        var userId = UserId.New();
        var plantId = PlantId.New();
        var noteId = PlantNoteId.New();
        var gardenMock = new Mock<IGardenRepository>(MockBehavior.Strict);
        var unitOfWorkMock = TestUnitOfWorkFactory.Create(garden: gardenMock.Object);

        gardenMock
            .Setup(repo => repo.RemovePlantNoteAsync(userId, plantId, noteId, cancellationToken))
            .ReturnsAsync(false);

        var useCase = new DeletePlantNoteUseCase(unitOfWorkMock.Object);

        // Act
        var result = await useCase.ExecuteAsync(userId, plantId.Value, noteId.Value, cancellationToken);

        // Assert
        result.Should().BeFalse();
    }

    [Fact(DisplayName = "Delete plant note saves when note is removed")]
    [Trait("Category", "Unit")]
    public async Task DeletePlantNoteWhenNoteIsRemovedSaves()
    {
        // Arrange
        var cancellationToken = new CancellationToken();
        var userId = UserId.New();
        var plantId = PlantId.New();
        var noteId = PlantNoteId.New();
        var gardenMock = new Mock<IGardenRepository>(MockBehavior.Strict);
        var unitOfWorkMock = TestUnitOfWorkFactory.Create(garden: gardenMock.Object);
        var saveCalls = 0;

        gardenMock
            .Setup(repo => repo.RemovePlantNoteAsync(userId, plantId, noteId, cancellationToken))
            .ReturnsAsync(true);
        unitOfWorkMock
            .Setup(work => work.SaveChangesAsync(cancellationToken))
            .Callback(() => saveCalls++)
            .Returns(Task.CompletedTask);

        var useCase = new DeletePlantNoteUseCase(unitOfWorkMock.Object);

        // Act
        var result = await useCase.ExecuteAsync(userId, plantId.Value, noteId.Value, cancellationToken);

        // Assert
        result.Should().BeTrue();
        saveCalls.Should().Be(1);
    }
}
