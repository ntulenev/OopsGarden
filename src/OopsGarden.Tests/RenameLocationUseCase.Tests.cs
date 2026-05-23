using Abstractions.Repositories;

using FluentAssertions;

using Logic.UseCases;

using Models;

using Moq;

namespace OopsGarden.Tests;

public sealed class RenameLocationUseCaseTests
{
    [Fact(DisplayName = "Rename location updates existing location")]
    [Trait("Category", "Unit")]
    public async Task RenameLocationWhenLocationExistsUpdatesLocation()
    {
        // Arrange
        var cancellationToken = new CancellationToken();
        var userId = UserId.New();
        var location = Location.Create(userId, LocationName.From("Kitchen"));
        var gardenMock = new Mock<IGardenRepository>(MockBehavior.Strict);
        var unitOfWorkMock = TestUnitOfWorkFactory.Create(garden: gardenMock.Object);
        var findCalls = 0;
        var saveCalls = 0;

        gardenMock
            .Setup(repo => repo.FindLocationAsync(userId, location.Id, cancellationToken))
            .Callback(() => findCalls++)
            .ReturnsAsync(location);
        unitOfWorkMock
            .Setup(work => work.SaveChangesAsync(cancellationToken))
            .Callback(() => saveCalls++)
            .Returns(Task.CompletedTask);

        var useCase = new RenameLocationUseCase(unitOfWorkMock.Object);

        // Act
        var result = await useCase.ExecuteAsync(userId, location.Id, new LocationCommand("Window"), cancellationToken);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Window");
        location.Name.Value.Should().Be("Window");
        findCalls.Should().Be(1);
        saveCalls.Should().Be(1);
    }

    [Fact(DisplayName = "Rename location returns null for missing location")]
    [Trait("Category", "Unit")]
    public async Task RenameLocationWhenLocationIsMissingReturnsNull()
    {
        // Arrange
        var cancellationToken = new CancellationToken();
        var userId = UserId.New();
        var locationId = LocationId.New();
        var gardenMock = new Mock<IGardenRepository>(MockBehavior.Strict);
        var unitOfWorkMock = TestUnitOfWorkFactory.Create(garden: gardenMock.Object);
        gardenMock.Setup(repo => repo.FindLocationAsync(userId, locationId, cancellationToken)).ReturnsAsync((Location?)null);
        var useCase = new RenameLocationUseCase(unitOfWorkMock.Object);

        // Act
        var result = await useCase.ExecuteAsync(userId, locationId, new LocationCommand("Window"), cancellationToken);

        // Assert
        result.Should().BeNull();
    }
}
