using Abstractions.Repositories;

using FluentAssertions;

using Logic.UseCases;

using Models;

using Moq;

namespace OopsGarden.Tests;

public sealed class LoginUseCaseTests
{
    [Fact(DisplayName = "Login returns authenticated user for valid credentials")]
    [Trait("Category", "Unit")]
    public async Task LoginWhenCredentialsAreValidReturnsUser()
    {
        // Arrange
        var cancellationToken = new CancellationToken();
        var user = TestUsers.Create("user@example.com");
        var passwords = new TestPasswordService();
        user.ChangePasswordHash(passwords.HashPassword(user, "secret"));

        var usersMock = new Mock<IUserRepository>(MockBehavior.Strict);
        var unitOfWorkMock = TestUnitOfWorkFactory.Create(usersMock.Object);
        var findCalls = 0;

        usersMock
            .Setup(repo => repo.FindByEmailAsync(UserEmail.From("USER@example.com"), cancellationToken))
            .Callback(() => findCalls++)
            .ReturnsAsync(user);

        var useCase = new LoginUseCase(unitOfWorkMock.Object, passwords);

        // Act
        var result = await useCase.ExecuteAsync(new LoginCommand("USER@example.com", "secret"), cancellationToken);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(user.Id);
        result.Email.Should().Be("USER@EXAMPLE.COM");
        findCalls.Should().Be(1);
    }

    [Theory(DisplayName = "Login returns null when credentials cannot authenticate")]
    [Trait("Category", "Unit")]
    [InlineData("missing@example.com", "secret")]
    [InlineData("user@example.com", "bad")]
    public async Task LoginWhenCredentialsAreInvalidReturnsNull(string email, string password)
    {
        // Arrange
        ArgumentNullException.ThrowIfNull(email);
        var cancellationToken = new CancellationToken();
        var passwords = new TestPasswordService();
        var user = TestUsers.Create("user@example.com");
        user.ChangePasswordHash(passwords.HashPassword(user, "secret"));
        var found = email.StartsWith("missing", StringComparison.Ordinal) ? null : user;

        var usersMock = new Mock<IUserRepository>(MockBehavior.Strict);
        var unitOfWorkMock = TestUnitOfWorkFactory.Create(usersMock.Object);
        var findCalls = 0;

        usersMock
            .Setup(repo => repo.FindByEmailAsync(UserEmail.From(email), cancellationToken))
            .Callback(() => findCalls++)
            .ReturnsAsync(found);

        var useCase = new LoginUseCase(unitOfWorkMock.Object, passwords);

        // Act
        var result = await useCase.ExecuteAsync(new LoginCommand(email, password), cancellationToken);

        // Assert
        result.Should().BeNull();
        findCalls.Should().Be(1);
    }
}
