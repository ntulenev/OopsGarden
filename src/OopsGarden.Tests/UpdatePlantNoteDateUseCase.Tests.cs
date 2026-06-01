using Abstractions.Repositories;

using FluentAssertions;

using Logic.UseCases;

using Models;

using Moq;

namespace OopsGarden.Tests;

public sealed class UpdatePlantNoteDateUseCaseTests
{
    [Fact(DisplayName = "Update plant note date returns false when note is missing")]
    [Trait("Category", "Unit")]
    public async Task UpdatePlantNoteDateWhenNoteIsMissingReturnsFalse()
    {
        // Arrange
        var cancellationToken = new CancellationToken();
        var userId = UserId.New();
        var plantId = PlantId.New();
        var noteId = PlantNoteId.New();
        var plantNotesMock = new Mock<IPlantNoteRepository>(MockBehavior.Strict);
        var unitOfWorkMock = TestUnitOfWorkFactory.Create(plantNotes: plantNotesMock.Object);
        var command = new UpdatePlantNoteDateCommand(new DateOnly(2026, 5, 23));

        plantNotesMock
            .Setup(repo => repo.UpdatePlantNoteCreatedAtAsync(
                userId,
                plantId,
                noteId,
                new DateTimeOffset(2026, 5, 23, 12, 0, 0, TimeSpan.Zero),
                cancellationToken))
            .ReturnsAsync(false);

        var useCase = new UpdatePlantNoteDateUseCase(unitOfWorkMock.Object);

        // Act
        var result = await useCase.ExecuteAsync(userId, plantId, noteId, command, cancellationToken);

        // Assert
        result.Should().BeFalse();
    }

    [Fact(DisplayName = "Update plant note date saves when note is updated")]
    [Trait("Category", "Unit")]
    public async Task UpdatePlantNoteDateWhenNoteIsUpdatedSaves()
    {
        // Arrange
        var cancellationToken = new CancellationToken();
        var userId = UserId.New();
        var plantId = PlantId.New();
        var noteId = PlantNoteId.New();
        var createdAt = new DateTimeOffset(2026, 5, 23, 12, 0, 0, TimeSpan.Zero);
        var plantNotesMock = new Mock<IPlantNoteRepository>(MockBehavior.Strict);
        var unitOfWorkMock = TestUnitOfWorkFactory.Create(plantNotes: plantNotesMock.Object);
        var saveCalls = 0;

        plantNotesMock
            .Setup(repo => repo.UpdatePlantNoteCreatedAtAsync(userId, plantId, noteId, createdAt, cancellationToken))
            .ReturnsAsync(true);
        unitOfWorkMock
            .Setup(work => work.SaveChangesAsync(cancellationToken))
            .Callback(() => saveCalls++)
            .Returns(Task.CompletedTask);

        var useCase = new UpdatePlantNoteDateUseCase(unitOfWorkMock.Object);

        // Act
        var result = await useCase.ExecuteAsync(
            userId,
            plantId,
            noteId,
            new UpdatePlantNoteDateCommand(new DateOnly(2026, 5, 23)),
            cancellationToken);

        // Assert
        result.Should().BeTrue();
        saveCalls.Should().Be(1);
    }
}
