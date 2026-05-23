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
        var cancellationToken = new CancellationToken();
        var userId = UserId.New();
        var plantId = PlantId.New();
        var gardenQueriesMock = new Mock<IGardenQueries>(MockBehavior.Strict);
        var unitOfWorkMock = TestUnitOfWorkFactory.Create(gardenQueries: gardenQueriesMock.Object);

        gardenQueriesMock
            .Setup(queries => queries.GetPublicGardenAsync(userId, cancellationToken))
            .ReturnsAsync((PublicGardenProjection?)null);

        var useCase = new ListPublicPlantHistoryUseCase(unitOfWorkMock.Object);

        var result = await useCase.ExecuteAsync(userId, plantId, cancellationToken);

        result.Should().BeNull();
    }

    [Fact(DisplayName = "List public plant history maps history for public garden plant")]
    [Trait("Category", "Unit")]
    public async Task ListPublicPlantHistoryWhenPlantIsPublicMapsHistory()
    {
        var cancellationToken = new CancellationToken();
        var userId = UserId.New();
        var plantId = PlantId.New();
        var item = new PlantHistoryItemProjection(Guid.NewGuid(), "watering", DateTimeOffset.UtcNow, null, false);
        var gardenQueriesMock = new Mock<IGardenQueries>(MockBehavior.Strict);
        var unitOfWorkMock = TestUnitOfWorkFactory.Create(gardenQueries: gardenQueriesMock.Object);

        gardenQueriesMock
            .Setup(queries => queries.GetPublicGardenAsync(userId, cancellationToken))
            .ReturnsAsync(new PublicGardenProjection(
                userId,
                "User",
                null,
                [new PublicGardenPlantProjection(plantId, "Basil", "Green", null, null, null, null)]));
        gardenQueriesMock
            .Setup(queries => queries.ListPlantHistoryAsync(userId, plantId, cancellationToken))
            .ReturnsAsync([item]);

        var useCase = new ListPublicPlantHistoryUseCase(unitOfWorkMock.Object);

        var result = await useCase.ExecuteAsync(userId, plantId, cancellationToken);

        result.Should().ContainSingle().Which.Id.Should().Be(item.Id);
    }
}
