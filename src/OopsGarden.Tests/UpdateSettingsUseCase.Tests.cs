
using FluentAssertions;

using Moq;

using OopsGarden.UseCases;

namespace OopsGarden.Tests;

public sealed class UpdateSettingsUseCaseTests
{
    [Fact(DisplayName = "Update settings updates active user")]
    [Trait("Category", "Unit")]
    public async Task UpdateSettingsWhenUserExistsUpdatesUser()
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
        unitOfWorkMock
            .Setup(work => work.SaveChangesAsync(cancellationToken))
            .Callback(() => saveCalls++)
            .Returns(Task.CompletedTask);

        var useCase = new UpdateSettingsUseCase(unitOfWorkMock.Object);

        // Act
        var result = await useCase.ExecuteAsync(
            user.Id,
            new SettingsCommand("New", "ru", "data:image/png;base64,abc", true),
            cancellationToken);

        // Assert
        result.Should().NotBeNull();
        result!.DisplayName.Should().Be("New");
        result.Language.Should().Be("ru");
        result.AvatarData.Should().Be("data:image/png;base64,abc");
        result.IsGardenPublic.Should().BeTrue();
        user.DisplayName.Value.Should().Be("New");
        findCalls.Should().Be(1);
        saveCalls.Should().Be(1);
    }

    [Fact(DisplayName = "Update settings returns null for blocked user")]
    [Trait("Category", "Unit")]
    public async Task UpdateSettingsWhenUserIsBlockedReturnsNull()
    {
        // Arrange
        var cancellationToken = new CancellationToken();
        var user = TestUsers.Create("user@example.com");
        user.Block();
        var usersMock = new Mock<IUserRepository>(MockBehavior.Strict);
        var unitOfWorkMock = TestUnitOfWorkFactory.Create(usersMock.Object);
        usersMock.Setup(repo => repo.FindByIdAsync(user.Id, cancellationToken)).ReturnsAsync(user);
        var useCase = new UpdateSettingsUseCase(unitOfWorkMock.Object);

        // Act
        var result = await useCase.ExecuteAsync(
            user.Id,
            new SettingsCommand("New", "ru", null, true),
            cancellationToken);

        // Assert
        result.Should().BeNull();
    }
}
