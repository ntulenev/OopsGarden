using Abstractions.Repositories;

using FluentAssertions;

using Logic.UseCases;

using Models;

using Moq;

namespace OopsGarden.Tests;

public sealed class DeleteLocationUseCaseTests
{
    [Fact(DisplayName = "Delete location clears plants removes location and saves")]
    [Trait("Category", "Unit")]
    public async Task DeleteLocationWhenLocationExistsClearsPlantsAndRemovesLocation()
    {
        // Arrange
        var cancellationToken = new CancellationToken();
        var userId = UserId.New();
        var location = Location.Create(userId, LocationName.From("Kitchen"));
        var gardenMock = new Mock<IGardenRepository>(MockBehavior.Strict);
        var unitOfWorkMock = TestUnitOfWorkFactory.Create(garden: gardenMock.Object);
        var clearCalls = 0;
        var removeCalls = 0;
        var saveCalls = 0;

        gardenMock.Setup(repo => repo.FindLocationAsync(userId, location.Id, cancellationToken)).ReturnsAsync(location);
        gardenMock
            .Setup(repo => repo.ClearPlantLocationAsync(userId, location.Id, cancellationToken))
            .Callback(() => clearCalls++)
            .Returns(Task.CompletedTask);
        gardenMock
            .Setup(repo => repo.RemoveLocation(location))
            .Callback(() => removeCalls++);
        unitOfWorkMock
            .Setup(work => work.SaveChangesAsync(cancellationToken))
            .Callback(() => saveCalls++)
            .Returns(Task.CompletedTask);

        var useCase = new DeleteLocationUseCase(unitOfWorkMock.Object);

        // Act
        var result = await useCase.ExecuteAsync(userId, location.Id, cancellationToken);

        // Assert
        result.Should().BeTrue();
        clearCalls.Should().Be(1);
        removeCalls.Should().Be(1);
        saveCalls.Should().Be(1);
    }

    [Fact(DisplayName = "Delete location returns false for missing location")]
    [Trait("Category", "Unit")]
    public async Task DeleteLocationWhenLocationIsMissingReturnsFalse()
    {
        // Arrange
        var cancellationToken = new CancellationToken();
        var userId = UserId.New();
        var locationId = LocationId.New();
        var gardenMock = new Mock<IGardenRepository>(MockBehavior.Strict);
        var unitOfWorkMock = TestUnitOfWorkFactory.Create(garden: gardenMock.Object);
        gardenMock.Setup(repo => repo.FindLocationAsync(userId, locationId, cancellationToken)).ReturnsAsync((Location?)null);
        var useCase = new DeleteLocationUseCase(unitOfWorkMock.Object);

        // Act
        var result = await useCase.ExecuteAsync(userId, locationId, cancellationToken);

        // Assert
        result.Should().BeFalse();
    }
}
