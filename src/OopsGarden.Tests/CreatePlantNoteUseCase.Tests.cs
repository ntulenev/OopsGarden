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
        var gardenMock = new Mock<IGardenRepository>(MockBehavior.Strict);
        var unitOfWorkMock = TestUnitOfWorkFactory.Create(garden: gardenMock.Object);

        gardenMock
            .Setup(repo => repo.FindPlantAsync(userId, plantId, cancellationToken))
            .ReturnsAsync((Plant?)null);

        var useCase = new CreatePlantNoteUseCase(unitOfWorkMock.Object);

        // Act
        var result = await useCase.ExecuteAsync(
            userId,
            plantId.Value,
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
        var gardenMock = new Mock<IGardenRepository>(MockBehavior.Strict);
        var unitOfWorkMock = TestUnitOfWorkFactory.Create(garden: gardenMock.Object);
        var addCalls = 0;
        var saveCalls = 0;

        gardenMock
            .Setup(repo => repo.FindPlantAsync(userId, plant.Id, cancellationToken))
            .ReturnsAsync(plant);
        gardenMock
            .Setup(repo => repo.AddPlantNoteAsync(It.Is<PlantNote>(note =>
                note.PlantId == plant.Id && note.Text.Value == "Sprouted"), cancellationToken))
            .Callback(() => addCalls++)
            .Returns(Task.CompletedTask);
        unitOfWorkMock
            .Setup(work => work.SaveChangesAsync(cancellationToken))
            .Callback(() => saveCalls++)
            .Returns(Task.CompletedTask);

        var useCase = new CreatePlantNoteUseCase(unitOfWorkMock.Object);

        // Act
        var result = await useCase.ExecuteAsync(
            userId,
            plant.Id.Value,
            new CreatePlantNoteCommand("Sprouted"),
            cancellationToken);

        // Assert
        result.Should().NotBeNull();
        result!.Text.Should().Be("Sprouted");
        result.Id.Value.Should().NotBe(Guid.Empty);
        result.CreatedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));
        plant.Notes.Should().ContainSingle();
        addCalls.Should().Be(1);
        saveCalls.Should().Be(1);
    }
}
