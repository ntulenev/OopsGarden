using Abstractions.Repositories;

using FluentAssertions;

using Logic.UseCases;

using Models;

using Moq;

namespace OopsGarden.Tests;

public sealed class ListPlantHistoryUseCaseTests
{
    [Fact(DisplayName = "List plant history returns null when plant is missing")]
    [Trait("Category", "Unit")]
    public async Task ListPlantHistoryWhenPlantIsMissingReturnsNull()
    {
        // Arrange
        var cancellationToken = new CancellationToken();
        var userId = UserId.New();
        var plantId = PlantId.New();
        var plantsMock = new Mock<IPlantRepository>(MockBehavior.Strict);
        var plantHistoryQueriesMock = new Mock<IPlantHistoryQueries>(MockBehavior.Strict);

        plantsMock
            .Setup(repo => repo.FindPlantAsync(userId, plantId, cancellationToken))
            .ReturnsAsync((Plant?)null);

        var useCase = new ListPlantHistoryUseCase(plantsMock.Object, plantHistoryQueriesMock.Object);

        // Act
        var result = await useCase.ExecuteAsync(userId, plantId, cancellationToken);

        // Assert
        result.Should().BeNull();
    }

    [Fact(DisplayName = "List plant history maps query items")]
    [Trait("Category", "Unit")]
    public async Task ListPlantHistoryWhenPlantExistsMapsQueryItems()
    {
        // Arrange
        var cancellationToken = new CancellationToken();
        var userId = UserId.New();
        var plant = Plant.Create(userId, PlantName.From("Basil"), PlantDescription.From(null), null, null, null);
        var plantsMock = new Mock<IPlantRepository>(MockBehavior.Strict);
        var plantHistoryQueriesMock = new Mock<IPlantHistoryQueries>(MockBehavior.Strict);
        var item = new PlantHistoryItemProjection(Guid.NewGuid(), "note", new DateTimeOffset(2026, 5, 23, 12, 0, 0, TimeSpan.Zero), "Sprouted", true);

        plantsMock
            .Setup(repo => repo.FindPlantAsync(userId, plant.Id, cancellationToken))
            .ReturnsAsync(plant);
        plantHistoryQueriesMock
            .Setup(queries => queries.ListPlantHistoryAsync(userId, plant.Id, cancellationToken))
            .ReturnsAsync([item]);

        var useCase = new ListPlantHistoryUseCase(plantsMock.Object, plantHistoryQueriesMock.Object);

        // Act
        var result = await useCase.ExecuteAsync(userId, plant.Id, cancellationToken);

        // Assert
        result.Should().ContainSingle().Which.Should().BeEquivalentTo(new PlantHistoryItem(
            item.Id,
            item.Type,
            item.OccurredAt,
            item.Text,
            item.IsAutomatic));
    }
}
