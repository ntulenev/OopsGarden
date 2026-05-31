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
        var clock = new TestClock();
        var today = DateOnly.FromDateTime(clock.UtcNow.UtcDateTime);
        var listCalls = 0;

        gardenQueriesMock
            .Setup(repo => repo.ListPlantsAsync(userId, today, cancellationToken))
            .Callback(() => listCalls++)
            .ReturnsAsync([
                new GardenPlantProjection(
                    PlantId.New(),
                    "Basil",
                    "Green",
                    string.Empty,
                    null,
                    null,
                    null,
                    DateTimeOffset.UtcNow,
                    true)
            ]);

        var useCase = new ListGardenPlantsUseCase(unitOfWorkMock.Object, clock);

        // Act
        var result = await useCase.ExecuteAsync(userId, cancellationToken);

        // Assert
        result.Should().ContainSingle();
        result[0].Name.Should().Be("Basil");
        result[0].HasOverdueReminders.Should().BeTrue();
        listCalls.Should().Be(1);
    }
}
