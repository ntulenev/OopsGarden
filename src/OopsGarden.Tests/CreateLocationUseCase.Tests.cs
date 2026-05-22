using Abstractions.Repositories;

using FluentAssertions;

using Logic.UseCases;

using Models;

using Moq;

namespace OopsGarden.Tests;

public sealed class CreateLocationUseCaseTests
{
    [Fact(DisplayName = "Create location persists location")]
    [Trait("Category", "Unit")]
    public async Task CreateLocationWhenCommandIsValidPersistsLocation()
    {
        // Arrange
        var cancellationToken = new CancellationToken();
        var userId = UserId.New();
        var gardenMock = new Mock<IGardenRepository>(MockBehavior.Strict);
        var unitOfWorkMock = TestUnitOfWorkFactory.Create(garden: gardenMock.Object);
        var addCalls = 0;
        var saveCalls = 0;

        gardenMock
            .Setup(repo => repo.AddLocationAsync(It.Is<Location>(location => location.UserId == userId), cancellationToken))
            .Callback(() => addCalls++)
            .Returns(Task.CompletedTask);
        unitOfWorkMock
            .Setup(work => work.SaveChangesAsync(cancellationToken))
            .Callback(() => saveCalls++)
            .Returns(Task.CompletedTask);

        var useCase = new CreateLocationUseCase(unitOfWorkMock.Object);

        // Act
        var result = await useCase.ExecuteAsync(userId, new LocationCommand("Kitchen"), cancellationToken);

        // Assert
        result.Name.Should().Be("Kitchen");
        addCalls.Should().Be(1);
        saveCalls.Should().Be(1);
    }
}
