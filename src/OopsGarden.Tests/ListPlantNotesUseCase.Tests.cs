using FluentAssertions;

using Models;

using Moq;

using OopsGarden.UseCases;

namespace OopsGarden.Tests;

public sealed class ListPlantNotesUseCaseTests
{
    [Fact(DisplayName = "List plant notes returns null when plant is missing")]
    [Trait("Category", "Unit")]
    public async Task ListPlantNotesWhenPlantIsMissingReturnsNull()
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

        var useCase = new ListPlantNotesUseCase(unitOfWorkMock.Object);

        // Act
        var result = await useCase.ExecuteAsync(userId, plantId.Value, 1, 5, cancellationToken);

        // Assert
        result.Should().BeNull();
    }

    [Fact(DisplayName = "List plant notes normalizes paging and maps notes")]
    [Trait("Category", "Unit")]
    public async Task ListPlantNotesWhenPlantExistsNormalizesPagingAndMapsNotes()
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
        var note = new PlantNoteProjection(PlantNoteId.New(), "Sprouted", DateTimeOffset.UtcNow);
        var gardenMock = new Mock<IGardenRepository>(MockBehavior.Strict);
        var unitOfWorkMock = TestUnitOfWorkFactory.Create(garden: gardenMock.Object);

        gardenMock
            .Setup(repo => repo.FindPlantAsync(userId, plant.Id, cancellationToken))
            .ReturnsAsync(plant);
        gardenMock
            .Setup(repo => repo.CountPlantNotesAsync(userId, plant.Id, cancellationToken))
            .ReturnsAsync(30);
        gardenMock
            .Setup(repo => repo.ListPlantNotesAsync(userId, plant.Id, 0, 20, cancellationToken))
            .ReturnsAsync([note]);

        var useCase = new ListPlantNotesUseCase(unitOfWorkMock.Object);

        // Act
        var result = await useCase.ExecuteAsync(userId, plant.Id.Value, 0, 50, cancellationToken);

        // Assert
        result.Should().NotBeNull();
        result!.Items.Should().ContainSingle().Which.Text.Should().Be("Sprouted");
        result.Page.Should().Be(1);
        result.PageSize.Should().Be(20);
        result.Total.Should().Be(30);
        result.HasPrevious.Should().BeFalse();
        result.HasNext.Should().BeTrue();
    }
}
