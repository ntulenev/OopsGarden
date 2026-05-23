using Abstractions.Repositories;

using FluentAssertions;

using Logic.UseCases;

using Models;

using Moq;

namespace OopsGarden.Tests;

public sealed class GetPublicGardenUseCaseTests
{
    [Fact(DisplayName = "Get public garden maps projection")]
    [Trait("Category", "Unit")]
    public async Task GetPublicGardenWhenProjectionExistsMapsGarden()
    {
        // Arrange
        var cancellationToken = new CancellationToken();
        var userId = UserId.New();
        var locationId = LocationId.New();
        var plantId = PlantId.New();
        var gardenQueriesMock = new Mock<IGardenQueries>(MockBehavior.Strict);
        var unitOfWorkMock = TestUnitOfWorkFactory.Create(gardenQueries: gardenQueriesMock.Object);
        var gardenCalls = 0;

        gardenQueriesMock
            .Setup(repo => repo.GetPublicGardenAsync(userId, cancellationToken))
            .Callback(() => gardenCalls++)
            .ReturnsAsync(new PublicGardenProjection(
                userId,
                "User",
                "avatar",
                [new PublicGardenPlantProjection(
                    plantId,
                    "Basil",
                    "Green",
                    "photo",
                    null,
                    null,
                    new GardenPlantLocationProjection(locationId, "Kitchen"))]));

        var useCase = new GetPublicGardenUseCase(unitOfWorkMock.Object);

        // Act
        var result = await useCase.ExecuteAsync(userId.Value, cancellationToken);

        // Assert
        result.Should().NotBeNull();
        result!.Plants.Should().ContainSingle();
        result.Plants[0].Location!.Name.Should().Be("Kitchen");
        gardenCalls.Should().Be(1);
    }
}
