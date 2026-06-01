using Abstractions.Repositories;

using FluentAssertions;

using Logic.UseCases;

using Models;

using Moq;

namespace OopsGarden.Tests;

public sealed class ListPublicPlantNotesUseCaseTests
{
    [Fact(DisplayName = "List public plant notes returns null when garden is private")]
    [Trait("Category", "Unit")]
    public async Task ListPublicPlantNotesWhenGardenIsMissingReturnsNull()
    {
        // Arrange
        var cancellationToken = new CancellationToken();
        var userId = UserId.New();
        var plantId = PlantId.New();
        var publicGardenQueriesMock = new Mock<IPublicGardenQueries>(MockBehavior.Strict);
        var plantNoteQueriesMock = new Mock<IPlantNoteQueries>(MockBehavior.Strict);

        publicGardenQueriesMock
            .Setup(queries => queries.PublicPlantExistsAsync(userId, plantId, cancellationToken))
            .ReturnsAsync(false);

        var useCase = new ListPublicPlantNotesUseCase(publicGardenQueriesMock.Object, plantNoteQueriesMock.Object);

        // Act
        var result = await useCase.ExecuteAsync(userId, plantId, 1, 5, cancellationToken);

        // Assert
        result.Should().BeNull();
    }

    [Fact(DisplayName = "List public plant notes returns null when plant is not in garden")]
    [Trait("Category", "Unit")]
    public async Task ListPublicPlantNotesWhenPlantIsMissingReturnsNull()
    {
        // Arrange
        var cancellationToken = new CancellationToken();
        var userId = UserId.New();
        var plantId = PlantId.New();
        var publicGardenQueriesMock = new Mock<IPublicGardenQueries>(MockBehavior.Strict);
        var plantNoteQueriesMock = new Mock<IPlantNoteQueries>(MockBehavior.Strict);

        publicGardenQueriesMock
            .Setup(queries => queries.PublicPlantExistsAsync(userId, plantId, cancellationToken))
            .ReturnsAsync(false);

        var useCase = new ListPublicPlantNotesUseCase(publicGardenQueriesMock.Object, plantNoteQueriesMock.Object);

        // Act
        var result = await useCase.ExecuteAsync(userId, plantId, 1, 5, cancellationToken);

        // Assert
        result.Should().BeNull();
    }

    [Fact(DisplayName = "List public plant notes maps notes for public garden plant")]
    [Trait("Category", "Unit")]
    public async Task ListPublicPlantNotesWhenPlantIsPublicMapsNotes()
    {
        // Arrange
        var cancellationToken = new CancellationToken();
        var userId = UserId.New();
        var plantId = PlantId.New();
        var note = new PlantNoteProjection(PlantNoteId.New(), "Sprouted", DateTimeOffset.UtcNow, false);
        var publicGardenQueriesMock = new Mock<IPublicGardenQueries>(MockBehavior.Strict);
        var plantNoteQueriesMock = new Mock<IPlantNoteQueries>(MockBehavior.Strict);

        publicGardenQueriesMock
            .Setup(queries => queries.PublicPlantExistsAsync(userId, plantId, cancellationToken))
            .ReturnsAsync(true);
        plantNoteQueriesMock
            .Setup(queries => queries.CountPlantNotesAsync(userId, plantId, cancellationToken))
            .ReturnsAsync(1);
        plantNoteQueriesMock
            .Setup(queries => queries.ListPlantNotesAsync(userId, plantId, 0, 5, cancellationToken))
            .ReturnsAsync([note]);

        var useCase = new ListPublicPlantNotesUseCase(publicGardenQueriesMock.Object, plantNoteQueriesMock.Object);

        // Act
        var result = await useCase.ExecuteAsync(userId, plantId, 1, 5, cancellationToken);

        // Assert
        result.Should().NotBeNull();
        result!.Items.Should().ContainSingle().Which.Text.Should().Be("Sprouted");
    }
}
