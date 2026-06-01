using Abstractions.Repositories;

using FluentAssertions;

using Logic.UseCases;

using Models;

using Moq;

namespace OopsGarden.Tests;

public sealed class ListPublicPlantHistoryUseCaseTests
{
    [Fact(DisplayName = "List public plant history returns null when garden is private")]
    [Trait("Category", "Unit")]
    public async Task ListPublicPlantHistoryWhenGardenIsMissingReturnsNull()
    {
        // Arrange
        var cancellationToken = new CancellationToken();
        var userId = UserId.New();
        var plantId = PlantId.New();
        var publicGardenQueriesMock = new Mock<IPublicGardenQueries>(MockBehavior.Strict);
        var plantHistoryQueriesMock = new Mock<IPlantHistoryQueries>(MockBehavior.Strict);

        publicGardenQueriesMock
            .Setup(queries => queries.PublicPlantExistsAsync(userId, plantId, cancellationToken))
            .ReturnsAsync(false);

        var useCase = new ListPublicPlantHistoryUseCase(publicGardenQueriesMock.Object, plantHistoryQueriesMock.Object);

        // Act
        var result = await useCase.ExecuteAsync(userId, plantId, cancellationToken);

        // Assert
        result.Should().BeNull();
    }

    [Fact(DisplayName = "List public plant history returns null when plant is not in garden")]
    [Trait("Category", "Unit")]
    public async Task ListPublicPlantHistoryWhenPlantIsMissingReturnsNull()
    {
        // Arrange
        var cancellationToken = new CancellationToken();
        var userId = UserId.New();
        var plantId = PlantId.New();
        var publicGardenQueriesMock = new Mock<IPublicGardenQueries>(MockBehavior.Strict);
        var plantHistoryQueriesMock = new Mock<IPlantHistoryQueries>(MockBehavior.Strict);

        publicGardenQueriesMock
            .Setup(queries => queries.PublicPlantExistsAsync(userId, plantId, cancellationToken))
            .ReturnsAsync(false);

        var useCase = new ListPublicPlantHistoryUseCase(publicGardenQueriesMock.Object, plantHistoryQueriesMock.Object);

        // Act
        var result = await useCase.ExecuteAsync(userId, plantId, cancellationToken);

        // Assert
        result.Should().BeNull();
    }

    [Fact(DisplayName = "List public plant history maps history for public garden plant")]
    [Trait("Category", "Unit")]
    public async Task ListPublicPlantHistoryWhenPlantIsPublicMapsHistory()
    {
        // Arrange
        var cancellationToken = new CancellationToken();
        var userId = UserId.New();
        var plantId = PlantId.New();
        var item = new PlantHistoryItemProjection(Guid.NewGuid(), "watering", DateTimeOffset.UtcNow, null, false);
        var publicGardenQueriesMock = new Mock<IPublicGardenQueries>(MockBehavior.Strict);
        var plantHistoryQueriesMock = new Mock<IPlantHistoryQueries>(MockBehavior.Strict);

        publicGardenQueriesMock
            .Setup(queries => queries.PublicPlantExistsAsync(userId, plantId, cancellationToken))
            .ReturnsAsync(true);
        plantHistoryQueriesMock
            .Setup(queries => queries.ListPlantHistoryAsync(userId, plantId, cancellationToken))
            .ReturnsAsync([item]);

        var useCase = new ListPublicPlantHistoryUseCase(publicGardenQueriesMock.Object, plantHistoryQueriesMock.Object);

        // Act
        var result = await useCase.ExecuteAsync(userId, plantId, cancellationToken);

        // Assert
        result.Should().ContainSingle().Which.Should().BeEquivalentTo(new PlantHistoryItem(
            item.Id,
            item.Type,
            item.OccurredAt,
            item.Text,
            item.IsAutomatic));
    }
}
