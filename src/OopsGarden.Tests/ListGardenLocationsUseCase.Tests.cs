
using FluentAssertions;


using Moq;

using Logic.UseCases;

namespace OopsGarden.Tests;

public sealed class ListGardenLocationsUseCaseTests
{
    [Fact(DisplayName = "List garden locations maps projections")]
    [Trait("Category", "Unit")]
    public async Task ListGardenLocationsWhenLocationsExistMapsLocations()
    {
        // Arrange
        var cancellationToken = new CancellationToken();
        var userId = UserId.New();
        var gardenMock = new Mock<IGardenRepository>(MockBehavior.Strict);
        var unitOfWorkMock = TestUnitOfWorkFactory.Create(garden: gardenMock.Object);
        var listCalls = 0;

        gardenMock
            .Setup(repo => repo.ListLocationsAsync(userId, cancellationToken))
            .Callback(() => listCalls++)
            .ReturnsAsync([new GardenLocationProjection(LocationId.New(), "Kitchen", 2)]);

        var useCase = new ListGardenLocationsUseCase(unitOfWorkMock.Object);

        // Act
        var result = await useCase.ExecuteAsync(userId, cancellationToken);

        // Assert
        result.Should().ContainSingle();
        result[0].Plants.Should().Be(2);
        listCalls.Should().Be(1);
    }
}
