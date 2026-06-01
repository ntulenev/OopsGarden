using Abstractions.Repositories;

using FluentAssertions;

using Logic.UseCases;

using Models;

using Moq;

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
        var plantNotesMock = new Mock<IPlantNoteRepository>(MockBehavior.Strict);
        var unitOfWorkMock = TestUnitOfWorkFactory.Create(plantNotes: plantNotesMock.Object);

        plantNotesMock
            .Setup(repo => repo.RemovePlantNoteAsync(userId, plantId, noteId, cancellationToken))
            .ReturnsAsync(false);

        var useCase = new DeletePlantNoteUseCase(unitOfWorkMock.Object);

        // Act
        var result = await useCase.ExecuteAsync(userId, plantId, noteId, cancellationToken);

        // Assert
        result.Status.Should().Be(CommandStatus.NotFound);
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
        var plantNotesMock = new Mock<IPlantNoteRepository>(MockBehavior.Strict);
        var unitOfWorkMock = TestUnitOfWorkFactory.Create(plantNotes: plantNotesMock.Object);
        var saveCalls = 0;

        plantNotesMock
            .Setup(repo => repo.RemovePlantNoteAsync(userId, plantId, noteId, cancellationToken))
            .ReturnsAsync(true);
        unitOfWorkMock
            .Setup(work => work.SaveChangesAsync(cancellationToken))
            .Callback(() => saveCalls++)
            .Returns(Task.CompletedTask);

        var useCase = new DeletePlantNoteUseCase(unitOfWorkMock.Object);

        // Act
        var result = await useCase.ExecuteAsync(userId, plantId, noteId, cancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        saveCalls.Should().Be(1);
    }
}
