
using FluentAssertions;


using Moq;

using Logic.UseCases;

namespace OopsGarden.Tests;

public sealed class ListInvitesUseCaseTests
{
    [Fact(DisplayName = "List invites maps projections")]
    [Trait("Category", "Unit")]
    public async Task ListInvitesWhenInvitesExistMapsInvites()
    {
        // Arrange
        var cancellationToken = new CancellationToken();
        var invitesMock = new Mock<IInviteRepository>(MockBehavior.Strict);
        var unitOfWorkMock = TestUnitOfWorkFactory.Create(invites: invitesMock.Object);
        var listCalls = 0;

        invitesMock
            .Setup(repo => repo.ListAsync(cancellationToken))
            .Callback(() => listCalls++)
            .ReturnsAsync([new AdminInviteProjection(
                InviteId.New(),
                "code",
                DateTimeOffset.UtcNow,
                "admin",
                null,
                null,
                false)]);

        var useCase = new ListInvitesUseCase(unitOfWorkMock.Object);

        // Act
        var result = await useCase.ExecuteAsync(cancellationToken);

        // Assert
        result.Should().ContainSingle();
        result[0].Code.Should().Be("code");
        listCalls.Should().Be(1);
    }
}
