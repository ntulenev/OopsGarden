using Abstractions;

using FluentAssertions;

using Models;

using Moq;

using OopsGarden.UseCases;

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
        var gardenMock = new Mock<IGardenRepository>(MockBehavior.Strict);
        var unitOfWorkMock = TestUnitOfWorkFactory.Create(garden: gardenMock.Object);
        var gardenCalls = 0;

        gardenMock
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
