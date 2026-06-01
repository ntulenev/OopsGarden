using Abstractions.Repositories;

using FluentAssertions;

using Logic.UseCases;

using Models;

using Moq;

namespace OopsGarden.Tests;

public sealed class CreatePlantNoteUseCaseTests
{
    [Fact(DisplayName = "Create plant note returns null when plant is missing")]
    [Trait("Category", "Unit")]
    public async Task CreatePlantNoteWhenPlantIsMissingReturnsNull()
    {
        // Arrange
        var cancellationToken = new CancellationToken();
        var userId = UserId.New();
        var plantId = PlantId.New();
        var plantsMock = new Mock<IPlantRepository>(MockBehavior.Strict);
        var plantNotesMock = new Mock<IPlantNoteRepository>(MockBehavior.Strict);
        var unitOfWorkMock = TestUnitOfWorkFactory.Create(plants: plantsMock.Object, plantNotes: plantNotesMock.Object);

        plantsMock
            .Setup(repo => repo.FindPlantAsync(userId, plantId, cancellationToken))
            .ReturnsAsync((Plant?)null);

        var clock = new TestClock();
        var useCase = new CreatePlantNoteUseCase(unitOfWorkMock.Object, clock);

        // Act
        var result = await useCase.ExecuteAsync(
            userId,
            plantId,
            new CreatePlantNoteCommand("Sprouted"),
            cancellationToken);

        // Assert
        result.Should().BeNull();
    }

    [Fact(DisplayName = "Create plant note adds note and saves")]
    [Trait("Category", "Unit")]
    public async Task CreatePlantNoteWhenPlantExistsAddsNoteAndSaves()
    {
        // Arrange
        var cancellationToken = new CancellationToken();
        var userId = UserId.New();
        var plant = Plant.Create(
            userId,
            PlantName.From("Basil"),
            PlantDescription.From("Green"),
            null,
            null,
            null);
        var plantsMock = new Mock<IPlantRepository>(MockBehavior.Strict);
        var plantNotesMock = new Mock<IPlantNoteRepository>(MockBehavior.Strict);
        var unitOfWorkMock = TestUnitOfWorkFactory.Create(plants: plantsMock.Object, plantNotes: plantNotesMock.Object);
        var addCalls = 0;
        var saveCalls = 0;

        plantsMock
            .Setup(repo => repo.FindPlantAsync(userId, plant.Id, cancellationToken))
            .ReturnsAsync(plant);
        plantNotesMock
            .Setup(repo => repo.AddPlantNoteAsync(It.Is<PlantNote>(note =>
                note.PlantId == plant.Id &&
                note.Text.Value == "Sprouted" &&
                note.Reminder.IsReminder &&
                note.Reminder.ReminderDate == new DateOnly(2026, 6, 1)), cancellationToken))
            .Callback(() => addCalls++)
            .Returns(Task.CompletedTask);
        unitOfWorkMock
            .Setup(work => work.SaveChangesAsync(cancellationToken))
            .Callback(() => saveCalls++)
            .Returns(Task.CompletedTask);

        var clock = new TestClock();
        var useCase = new CreatePlantNoteUseCase(unitOfWorkMock.Object, clock);

        // Act
        var result = await useCase.ExecuteAsync(
            userId,
            plant.Id,
            new CreatePlantNoteCommand("Sprouted", IsReminder: true, ReminderDate: new DateOnly(2026, 6, 1)),
            cancellationToken);

        // Assert
        result.Should().NotBeNull();
        result!.Text.Should().Be("Sprouted");
        result.Id.Value.Should().NotBe(Guid.Empty);
        result.CreatedAt.Should().Be(clock.UtcNow);
        result.IsReminder.Should().BeTrue();
        result.ReminderDate.Should().Be(new DateOnly(2026, 6, 1));
        result.IsReminderResolved.Should().BeFalse();
        plant.Notes.Should().ContainSingle();
        addCalls.Should().Be(1);
        saveCalls.Should().Be(1);
    }
}
