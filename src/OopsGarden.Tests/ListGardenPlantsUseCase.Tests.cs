using Abstractions.Repositories;

using FluentAssertions;

using Logic.UseCases;

using Models;

using Moq;

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
        var gardenQueriesMock = new Mock<IGardenQueries>(MockBehavior.Strict);
        var unitOfWorkMock = TestUnitOfWorkFactory.Create(gardenQueries: gardenQueriesMock.Object);
        var listCalls = 0;

        gardenQueriesMock
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
