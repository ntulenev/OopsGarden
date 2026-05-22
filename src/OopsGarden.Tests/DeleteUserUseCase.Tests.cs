
using FluentAssertions;

using Models;

using Moq;

using OopsGarden.UseCases;

namespace OopsGarden.Tests;

public sealed class DeleteUserUseCaseTests
{
    [Fact(DisplayName = "Delete user removes existing user")]
    [Trait("Category", "Unit")]
    public async Task DeleteUserWhenUserExistsRemovesUser()
    {
        // Arrange
        var cancellationToken = new CancellationToken();
        var user = TestUsers.Create("user@example.com");
        var usersMock = new Mock<IUserRepository>(MockBehavior.Strict);
        var unitOfWorkMock = TestUnitOfWorkFactory.Create(usersMock.Object);
        var removeCalls = 0;
        var saveCalls = 0;

        usersMock.Setup(repo => repo.FindByIdAsync(user.Id, cancellationToken)).ReturnsAsync(user);
        usersMock.Setup(repo => repo.Remove(user)).Callback(() => removeCalls++);
        unitOfWorkMock.Setup(work => work.SaveChangesAsync(cancellationToken)).Callback(() => saveCalls++).Returns(Task.CompletedTask);
        var useCase = new DeleteUserUseCase(unitOfWorkMock.Object);

        // Act
        var result = await useCase.ExecuteAsync(user.Id.Value, cancellationToken);

        // Assert
        result.Should().BeTrue();
        removeCalls.Should().Be(1);
        saveCalls.Should().Be(1);
    }

    [Fact(DisplayName = "Delete user returns false for missing user")]
    [Trait("Category", "Unit")]
    public async Task DeleteUserWhenUserIsMissingReturnsFalse()
    {
        // Arrange
        var cancellationToken = new CancellationToken();
        var userId = UserId.New();
        var usersMock = new Mock<IUserRepository>(MockBehavior.Strict);
        var unitOfWorkMock = TestUnitOfWorkFactory.Create(usersMock.Object);
        usersMock.Setup(repo => repo.FindByIdAsync(userId, cancellationToken)).ReturnsAsync((AppUser?)null);
        var useCase = new DeleteUserUseCase(unitOfWorkMock.Object);

        // Act
        var result = await useCase.ExecuteAsync(userId.Value, cancellationToken);

        // Assert
        result.Should().BeFalse();
    }
}
