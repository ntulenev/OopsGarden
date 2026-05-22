
using FluentAssertions;

using Models;

using Moq;

using OopsGarden.UseCases;

namespace OopsGarden.Tests;

public sealed class RevokeInviteUseCaseTests
{
    [Fact(DisplayName = "Revoke invite returns false for missing invite")]
    [Trait("Category", "Unit")]
    public async Task RevokeInviteWhenInviteIsMissingReturnsFalse()
    {
        // Arrange
        var cancellationToken = new CancellationToken();
        var inviteId = InviteId.New();
        var invitesMock = new Mock<IInviteRepository>(MockBehavior.Strict);
        var unitOfWorkMock = TestUnitOfWorkFactory.Create(invites: invitesMock.Object);
        invitesMock.Setup(repo => repo.FindByIdAsync(inviteId, cancellationToken)).ReturnsAsync((InviteLink?)null);
        var useCase = new RevokeInviteUseCase(unitOfWorkMock.Object);

        // Act
        var result = await useCase.ExecuteAsync(inviteId.Value, cancellationToken);

        // Assert
        result.Should().BeFalse();
    }

    [Fact(DisplayName = "ExecuteAsync revokes existing invite")]
    [Trait("Category", "Unit")]
    public async Task ExecuteAsyncWhenInviteExistsRevokesInvite()
    {
        // Arrange
        var cancellationToken = new CancellationToken();
        var invite = InviteLink.Create(InviteCode.From("code"), AdminName.From("admin"));
        var invitesMock = new Mock<IInviteRepository>(MockBehavior.Strict);
        var unitOfWorkMock = TestUnitOfWorkFactory.Create(invites: invitesMock.Object);
        var findCalls = 0;
        var saveCalls = 0;

        invitesMock
            .Setup(repo => repo.FindByIdAsync(invite.Id, cancellationToken))
            .Callback(() => findCalls++)
            .ReturnsAsync(invite);
        unitOfWorkMock
            .Setup(work => work.SaveChangesAsync(cancellationToken))
            .Callback(() => saveCalls++)
            .Returns(Task.CompletedTask);

        var useCase = new RevokeInviteUseCase(unitOfWorkMock.Object);

        // Act
        var result = await useCase.ExecuteAsync(invite.Id.Value, cancellationToken);

        // Assert
        result.Should().BeTrue();
        invite.IsRevoked.Should().BeTrue();
        findCalls.Should().Be(1);
        saveCalls.Should().Be(1);
    }
}
