using System.Security.Claims;

using Abstractions.Repositories;

using FluentAssertions;

using Logic.UseCases;

using Moq;

namespace OopsGarden.Tests;

public sealed class GetMeUseCaseTests
{
    [Fact(DisplayName = "GetMe returns anonymous current user for anonymous principal")]
    [Trait("Category", "Unit")]
    public async Task GetMeWhenPrincipalIsAnonymousReturnsAnonymousUser()
    {
        // Arrange
        var cancellationToken = new CancellationToken();
        var unitOfWorkMock = TestUnitOfWorkFactory.Create();
        var useCase = new GetMeUseCase(unitOfWorkMock.Object);

        // Act
        var result = await useCase.ExecuteAsync(new ClaimsPrincipal(), cancellationToken);

        // Assert
        result.Authenticated.Should().BeFalse();
    }

    [Fact(DisplayName = "GetMe returns current admin for admin principal")]
    [Trait("Category", "Unit")]
    public async Task GetMeWhenPrincipalIsAdminReturnsCurrentAdmin()
    {
        // Arrange
        var cancellationToken = new CancellationToken();
        var unitOfWorkMock = TestUnitOfWorkFactory.Create();
        var principal = new ClaimsPrincipal(new ClaimsIdentity(
            [
                new Claim(ClaimTypes.Name, "admin"),
                new Claim(ClaimTypes.Role, "Admin")
            ],
            "Test"));
        var useCase = new GetMeUseCase(unitOfWorkMock.Object);

        // Act
        var result = await useCase.ExecuteAsync(principal, cancellationToken);

        // Assert
        result.Authenticated.Should().BeTrue();
        result.Name.Should().Be("admin");
        result.Role.Should().Be("Admin");
        result.Language.Should().Be("en");
    }

    [Fact(DisplayName = "GetMe returns current user for authenticated user principal")]
    [Trait("Category", "Unit")]
    public async Task GetMeWhenPrincipalIsUserReturnsCurrentUser()
    {
        // Arrange
        var cancellationToken = new CancellationToken();
        var user = TestUsers.Create("user@example.com");
        var usersMock = new Mock<IUserRepository>(MockBehavior.Strict);
        var unitOfWorkMock = TestUnitOfWorkFactory.Create(usersMock.Object);
        var findCalls = 0;

        usersMock
            .Setup(repo => repo.FindByIdAsync(user.Id, cancellationToken))
            .Callback(() => findCalls++)
            .ReturnsAsync(user);

        var principal = TestUsers.CreatePrincipal(user.Id, "User");
        var useCase = new GetMeUseCase(unitOfWorkMock.Object);

        // Act
        var result = await useCase.ExecuteAsync(principal, cancellationToken);

        // Assert
        result.Authenticated.Should().BeTrue();
        result.Id.Should().Be(user.Id);
        result.Name.Should().Be("User");
        result.Role.Should().Be("User");
        findCalls.Should().Be(1);
    }
}
