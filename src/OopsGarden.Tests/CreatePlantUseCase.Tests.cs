using Abstractions.Repositories;

using FluentAssertions;

using Logic.UseCases;

using Models;

using Moq;

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
        var locationsMock = new Mock<ILocationRepository>(MockBehavior.Strict);
        var unitOfWorkMock = TestUnitOfWorkFactory.Create(locations: locationsMock.Object);
        var locationExistsCalls = 0;

        locationsMock
            .Setup(repo => repo.LocationExistsAsync(userId, locationId, cancellationToken))
            .Callback(() => locationExistsCalls++)
            .ReturnsAsync(false);

        var useCase = new CreatePlantUseCase(unitOfWorkMock.Object, new TestClock());

        // Act
        var result = await useCase.ExecuteAsync(
            userId,
            new PlantCommand("Basil", "Green", locationId.Value, null, null, null),
            cancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(PlantCommandError.InvalidLocation);
        result.ErrorMessage.Should().Be("Invalid location.");
        locationExistsCalls.Should().Be(1);
    }

    [Fact(DisplayName = "Create plant persists plant when command is valid")]
    [Trait("Category", "Unit")]
    public async Task CreatePlantWhenCommandIsValidPersistsPlant()
    {
        // Arrange
        var cancellationToken = new CancellationToken();
        var userId = UserId.New();
        var plantsMock = new Mock<IPlantRepository>(MockBehavior.Strict);
        var unitOfWorkMock = TestUnitOfWorkFactory.Create(plants: plantsMock.Object);
        var addCalls = 0;
        var saveCalls = 0;

        plantsMock
            .Setup(repo => repo.AddPlantAsync(It.Is<Plant>(plant => plant.UserId == userId), cancellationToken))
            .Callback(() => addCalls++)
            .Returns(Task.CompletedTask);
        unitOfWorkMock
            .Setup(work => work.SaveChangesAsync(cancellationToken))
            .Callback(() => saveCalls++)
            .Returns(Task.CompletedTask);

        var useCase = new CreatePlantUseCase(unitOfWorkMock.Object, new TestClock());

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

    [Fact(DisplayName = "Create plant records initial photo history")]
    [Trait("Category", "Unit")]
    public async Task CreatePlantWhenPhotoIsProvidedRecordsPhotoHistory()
    {
        // Arrange
        var cancellationToken = new CancellationToken();
        var userId = UserId.New();
        var photoData = "data:image/png;base64,basil";
        var clock = new TestClock();
        var plantsMock = new Mock<IPlantRepository>(MockBehavior.Strict);
        var unitOfWorkMock = TestUnitOfWorkFactory.Create(plants: plantsMock.Object);
        var photoCalls = 0;

        plantsMock
            .Setup(repo => repo.AddPlantAsync(It.Is<Plant>(plant => plant.UserId == userId), cancellationToken))
            .Returns(Task.CompletedTask);
        plantsMock
            .Setup(repo => repo.AddPlantPhotoAsync(
                It.IsAny<PlantId>(),
                It.Is<ImageDataUrl>(photo => photo.Value == photoData),
                clock.UtcNow,
                cancellationToken))
            .Callback(() => photoCalls++)
            .Returns(Task.CompletedTask);
        unitOfWorkMock
            .Setup(work => work.SaveChangesAsync(cancellationToken))
            .Returns(Task.CompletedTask);

        var useCase = new CreatePlantUseCase(unitOfWorkMock.Object, clock);

        // Act
        var result = await useCase.ExecuteAsync(
            userId,
            new PlantCommand("Basil", "Green", null, null, null, photoData),
            cancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        photoCalls.Should().Be(1);
    }
}
