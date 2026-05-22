
using FluentAssertions;


using Moq;

using Logic.UseCases;

namespace OopsGarden.Tests;

public sealed class CreatePlantUseCaseTests
{
    [Fact(DisplayName = "Create plant returns invalid location error")]
    [Trait("Category", "Unit")]
    public async Task CreatePlantWhenLocationDoesNotExistReturnsError()
    {
        // Arrange
        var cancellationToken = new CancellationToken();
        var userId = UserId.New();
        var locationId = LocationId.New();
        var gardenMock = new Mock<IGardenRepository>(MockBehavior.Strict);
        var unitOfWorkMock = TestUnitOfWorkFactory.Create(garden: gardenMock.Object);
        var locationExistsCalls = 0;

        gardenMock
            .Setup(repo => repo.LocationExistsAsync(userId, locationId, cancellationToken))
            .Callback(() => locationExistsCalls++)
            .ReturnsAsync(false);

        var useCase = new CreatePlantUseCase(unitOfWorkMock.Object);

        // Act
        var result = await useCase.ExecuteAsync(
            userId,
            new PlantCommand("Basil", "Green", locationId.Value, null, null, null),
            cancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Invalid location.");
        locationExistsCalls.Should().Be(1);
    }

    [Fact(DisplayName = "Create plant persists plant when command is valid")]
    [Trait("Category", "Unit")]
    public async Task CreatePlantWhenCommandIsValidPersistsPlant()
    {
        // Arrange
        var cancellationToken = new CancellationToken();
        var userId = UserId.New();
        var gardenMock = new Mock<IGardenRepository>(MockBehavior.Strict);
        var unitOfWorkMock = TestUnitOfWorkFactory.Create(garden: gardenMock.Object);
        var addCalls = 0;
        var saveCalls = 0;

        gardenMock
            .Setup(repo => repo.AddPlantAsync(It.Is<Plant>(plant => plant.UserId == userId), cancellationToken))
            .Callback(() => addCalls++)
            .Returns(Task.CompletedTask);
        unitOfWorkMock
            .Setup(work => work.SaveChangesAsync(cancellationToken))
            .Callback(() => saveCalls++)
            .Returns(Task.CompletedTask);

        var useCase = new CreatePlantUseCase(unitOfWorkMock.Object);

        // Act
        var result = await useCase.ExecuteAsync(
            userId,
            new PlantCommand("Basil", "Green", null, null, null, null),
            cancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Id.Should().NotBeNull();
        addCalls.Should().Be(1);
        saveCalls.Should().Be(1);
    }
}
