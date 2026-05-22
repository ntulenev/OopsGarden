using Abstractions.Repositories;

using FluentAssertions;

using Logic.UseCases;

using Models;

using Moq;

namespace OopsGarden.Tests;

public sealed class BlockUserUseCaseTests
{
    [Fact(DisplayName = "ExecuteAsync blocks existing user")]
    [Trait("Category", "Unit")]
    public async Task ExecuteAsyncWhenUserExistsBlocksUser()
    {
        // Arrange
        var cancellationToken = new CancellationToken();
        var user = TestUsers.Create("user@example.com");
        var usersMock = new Mock<IUserRepository>(MockBehavior.Strict);
        var unitOfWorkMock = TestUnitOfWorkFactory.Create(usersMock.Object);
        var findCalls = 0;
        var saveCalls = 0;

        usersMock
            .Setup(repo => repo.FindByIdAsync(user.Id, cancellationToken))
            .Callback(() => findCalls++)
            .ReturnsAsync(user);
        unitOfWorkMock.Setup(work => work.SaveChangesAsync(cancellationToken)).Callback(() => saveCalls++).Returns(Task.CompletedTask);

        // Act
        var result = await new BlockUserUseCase(unitOfWorkMock.Object).ExecuteAsync(user.Id.Value, true, cancellationToken);

        // Assert
        result.Should().BeTrue();
        user.IsBlocked.Should().BeTrue();
        findCalls.Should().Be(1);
        saveCalls.Should().Be(1);
    }

    [Fact(DisplayName = "ExecuteAsync unblocks existing user")]
    [Trait("Category", "Unit")]
    public async Task ExecuteAsyncWhenUserExistsUnblocksUser()
    {
        // Arrange
        var cancellationToken = new CancellationToken();
        var user = TestUsers.Create("user@example.com");
        user.Block();
        var usersMock = new Mock<IUserRepository>(MockBehavior.Strict);
        var unitOfWorkMock = TestUnitOfWorkFactory.Create(usersMock.Object);
        var findCalls = 0;
        var saveCalls = 0;

        usersMock
            .Setup(repo => repo.FindByIdAsync(user.Id, cancellationToken))
            .Callback(() => findCalls++)
            .ReturnsAsync(user);
        unitOfWorkMock.Setup(work => work.SaveChangesAsync(cancellationToken)).Callback(() => saveCalls++).Returns(Task.CompletedTask);

        // Act
        var result = await new BlockUserUseCase(unitOfWorkMock.Object).ExecuteAsync(user.Id.Value, false, cancellationToken);

        // Assert
        result.Should().BeTrue();
        user.IsBlocked.Should().BeFalse();
        findCalls.Should().Be(1);
        saveCalls.Should().Be(1);
    }

    [Fact(DisplayName = "Block user returns false for missing user")]
    [Trait("Category", "Unit")]
    public async Task BlockUserWhenUserIsMissingReturnsFalse()
    {
        // Arrange
        var cancellationToken = new CancellationToken();
        var userId = UserId.New();
        var usersMock = new Mock<IUserRepository>(MockBehavior.Strict);
        var unitOfWorkMock = TestUnitOfWorkFactory.Create(usersMock.Object);
        usersMock.Setup(repo => repo.FindByIdAsync(userId, cancellationToken)).ReturnsAsync((AppUser?)null);

        // Act
        var result = await new BlockUserUseCase(unitOfWorkMock.Object).ExecuteAsync(userId.Value, true, cancellationToken);

        // Assert
        result.Should().BeFalse();
    }
}
