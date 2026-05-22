
using FluentAssertions;

using Models;

using Moq;

using OopsGarden.UseCases;

namespace OopsGarden.Tests;

public sealed class ListGardenPlantsUseCaseTests
{
    [Fact(DisplayName = "List garden plants maps projections")]
    [Trait("Category", "Unit")]
    public async Task ListGardenPlantsWhenPlantsExistMapsPlants()
    {
        // Arrange
        var cancellationToken = new CancellationToken();
        var userId = UserId.New();
        var gardenMock = new Mock<IGardenRepository>(MockBehavior.Strict);
        var unitOfWorkMock = TestUnitOfWorkFactory.Create(garden: gardenMock.Object);
        var listCalls = 0;

        gardenMock
            .Setup(repo => repo.ListPlantsAsync(userId, cancellationToken))
            .Callback(() => listCalls++)
            .ReturnsAsync([
                new GardenPlantProjection(
                    PlantId.New(),
                    "Basil",
                    "Green",
                    null,
                    null,
                    null,
                    DateTimeOffset.UtcNow)
            ]);

        var useCase = new ListGardenPlantsUseCase(unitOfWorkMock.Object);

        // Act
        var result = await useCase.ExecuteAsync(userId, cancellationToken);

        // Assert
        result.Should().ContainSingle();
        result[0].Name.Should().Be("Basil");
        listCalls.Should().Be(1);
    }
}
