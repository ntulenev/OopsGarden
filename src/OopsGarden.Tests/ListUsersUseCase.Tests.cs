
using FluentAssertions;


using Moq;

using Logic.UseCases;

namespace OopsGarden.Tests;

public sealed class ListUsersUseCaseTests
{
    [Fact(DisplayName = "List users maps projections")]
    [Trait("Category", "Unit")]
    public async Task ListUsersWhenUsersExistMapsUsers()
    {
        // Arrange
        var cancellationToken = new CancellationToken();
        var usersMock = new Mock<IUserRepository>(MockBehavior.Strict);
        var unitOfWorkMock = TestUnitOfWorkFactory.Create(usersMock.Object);
        var listCalls = 0;

        usersMock
            .Setup(repo => repo.ListAdminUsersAsync(cancellationToken))
            .Callback(() => listCalls++)
            .ReturnsAsync([new AdminUserProjection(
                UserId.New(),
                "User",
                "USER@EXAMPLE.COM",
                false,
                "en",
                DateTimeOffset.UtcNow,
                3)]);

        var useCase = new ListUsersUseCase(unitOfWorkMock.Object);

        // Act
        var result = await useCase.ExecuteAsync(cancellationToken);

        // Assert
        result.Should().ContainSingle();
        result[0].Plants.Should().Be(3);
        listCalls.Should().Be(1);
    }
}
